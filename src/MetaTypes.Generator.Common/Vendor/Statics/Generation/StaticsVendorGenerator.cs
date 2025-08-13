using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using MetaTypes.Generator.Common.Generator;

namespace MetaTypes.Generator.Common.Vendor.Statics.Generation
{
    /// <summary>
    /// Statics vendor generator that generates partial class extensions with static service method metadata
    /// </summary>
    public class StaticsVendorGenerator : IVendorGenerator
    {
        private StaticsConfig _config = new();
        
        public string VendorName => "Statics";
        
        public string Description => "Generates Statics-specific metadata extensions for static service classes";
        
        /// <summary>
        /// Configure the Statics vendor generator with its specific configuration
        /// </summary>
        public void Configure(JsonElement? config)
        {
            if (config.HasValue)
            {
                try
                {
                    _config = JsonSerializer.Deserialize<StaticsConfig>(config.Value) ?? new StaticsConfig();
                }
                catch
                {
                    // Use default config if parsing fails
                    _config = new StaticsConfig();
                }
            }
            else
            {
                // Use default config when no config provided
                _config = new StaticsConfig();
            }
        }

        /// <summary>
        /// Generates Statics extensions only for types discovered by Statics discovery methods
        /// </summary>
        public IEnumerable<GeneratedFile> Generate(
            IEnumerable<DiscoveredType> discoveredTypes,
            Compilation compilation,
            GeneratorContext context)
        {
            // Check if base types are required and available
            var baseTypesAvailable = context.Properties.TryGetValue("BaseMetaTypesGenerated", out var baseGenerated) 
                && bool.Parse(baseGenerated);
                
            if (_config.RequireBaseTypes && !baseTypesAvailable)
            {
                // If base types are required but not available, skip vendor generation
                yield break;
            }

            // Filter to only types discovered by Statics discovery methods
            var staticsTypes = discoveredTypes
                .Where(dt => dt.WasDiscoveredByPrefix("Statics."))
                .Select(dt => dt.TypeSymbol)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>()
                .ToList();

            if (!staticsTypes.Any())
            {
                yield break;
            }

            // Generate Statics extensions for each discovered static class
            foreach (var staticClass in staticsTypes)
            {
                var source = GenerateStaticsExtension(staticClass);
                var assemblyName = staticClass.ContainingAssembly.Name;
                yield return new GeneratedFile
                {
                    FileName = $"{assemblyName}_{staticClass.Name}MetaTypeStatics.g.cs",
                    Content = source
                };
            }
        }

        private string GenerateStaticsExtension(INamedTypeSymbol typeSymbol)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine("using global::Statics.ServiceBroker.Attributes;");
            sb.AppendLine();
            
            // Use the assembly name for the MetaType namespace to match the base generator
            var assemblyName = typeSymbol.ContainingAssembly.Name;
            sb.AppendLine($"namespace {assemblyName};");
            sb.AppendLine();
            
            // Get static methods with StaticsServiceMethodAttribute
            var serviceMethods = GetStaticServiceMethods(typeSymbol);
            
            // Generate partial class extension for the MetaType class with Statics interface
            sb.AppendLine($"public partial class {typeSymbol.Name}MetaType : IMetaTypeStatics");
            sb.AppendLine("{");
            
            // Generate ServiceMethods collection
            sb.AppendLine("    public IReadOnlyList<IStaticsServiceMethod> ServiceMethods => [");
            foreach (var method in serviceMethods)
            {
                sb.AppendLine($"        new {typeSymbol.Name}ServiceMethod{method.Name}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate service method implementations
            foreach (var method in serviceMethods)
            {
                sb.AppendLine($"#region {method.Name} Service Method");
                sb.AppendLine();
                GenerateServiceMethodClass(sb, typeSymbol, method);
                sb.AppendLine("#endregion");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private void GenerateServiceMethodClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method)
        {
            sb.AppendLine($"public class {typeSymbol.Name}ServiceMethod{method.Name} : IStaticsServiceMethod");
            sb.AppendLine("{");
            
            // Method name
            sb.AppendLine($"    public string MethodName => \"{method.Name}\";");
            
            // Return type
            // Handle nullable reference types properly for typeof
            var returnTypeString = method.ReturnType.ToDisplayString();
            if (returnTypeString.EndsWith("?") && !method.ReturnType.IsValueType)
            {
                // Remove nullable annotation for typeof since it can't handle nullable reference types
                returnTypeString = returnTypeString.TrimEnd('?');
            }
            sb.AppendLine($"    public Type ReturnType => typeof({returnTypeString});");
            
            // Method attributes
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeInfo> MethodAttributes => [");
            foreach (var attr in method.GetAttributes())
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Attribute{GetSimpleAttributeName(attr)}(),");
            }
            sb.AppendLine("    ];");
            
            // Parameters
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsParameterInfo> Parameters => [");
            foreach (var param in method.Parameters)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(param.Name)}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate attribute classes for this method
            foreach (var attr in method.GetAttributes())
            {
                GenerateAttributeClass(sb, typeSymbol, method, attr);
            }
            
            // Generate parameter classes for this method
            foreach (var param in method.Parameters)
            {
                GenerateParameterClass(sb, typeSymbol, method, param);
            }
        }

        private void GenerateAttributeClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, AttributeData attribute)
        {
            var attrName = GetSimpleAttributeName(attribute);
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Attribute{attrName} : IStaticsAttributeInfo");
            sb.AppendLine("{");
            
            // Attribute type
            var attrTypeString = attribute.AttributeClass?.ToDisplayString();
            // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
            if (attrTypeString != null && attrTypeString.StartsWith("Statics."))
            {
                attrTypeString = "global::" + attrTypeString;
            }
            sb.AppendLine($"    public Type AttributeType => typeof({attrTypeString});");
            
            // Constructor arguments
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => [");
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Attribute{attrName}ConstructorArg{i}(),");
            }
            sb.AppendLine("    ];");
            
            // Named arguments
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => [");
            foreach (var namedArg in attribute.NamedArguments)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Attribute{attrName}NamedArg{namedArg.Key}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate constructor argument classes
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                var arg = attribute.ConstructorArguments[i];
                sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Attribute{attrName}ConstructorArg{i} : IStaticsAttributeArgument");
                sb.AppendLine("{");
                var argTypeString = arg.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                sb.AppendLine($"    public Type ArgumentType => typeof({argTypeString});");
                
                // Generate type-specific Value property
                var valueType = GetValuePropertyType(arg.Type);
                var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                sb.AppendLine($"    public {valueType} Value => {formattedValue};");
                
                sb.AppendLine("}");
                sb.AppendLine();
            }
            
            // Generate named argument classes
            foreach (var namedArg in attribute.NamedArguments)
            {
                sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Attribute{attrName}NamedArg{namedArg.Key} : IStaticsAttributeNamedArgument");
                sb.AppendLine("{");
                sb.AppendLine($"    public string Name => \"{namedArg.Key}\";");
                var argTypeString = namedArg.Value.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && namedArg.Value.Type != null && !namedArg.Value.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
                if (argTypeString != null && argTypeString.StartsWith("Statics."))
                {
                    argTypeString = "global::" + argTypeString;
                }
                sb.AppendLine($"    public Type ArgumentType => typeof({argTypeString});");
                
                // Generate type-specific Value property
                var valueType = GetValuePropertyType(namedArg.Value.Type);
                var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                sb.AppendLine($"    public {valueType} Value => {formattedValue};");
                
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        private void GenerateParameterClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, IParameterSymbol parameter)
        {
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)} : IStaticsParameterInfo");
            sb.AppendLine("{");
            
            // Parameter name and type
            sb.AppendLine($"    public string ParameterName => \"{parameter.Name}\";");
            // Handle nullable reference types properly for typeof
            var parameterTypeString = parameter.Type.ToDisplayString();
            if (parameterTypeString.EndsWith("?") && !parameter.Type.IsValueType)
            {
                // Remove nullable annotation for typeof since it can't handle nullable reference types
                parameterTypeString = parameterTypeString.TrimEnd('?');
            }
            sb.AppendLine($"    public Type ParameterType => typeof({parameterTypeString});");
            
            // Parameter attributes
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes => [");
            foreach (var attr in parameter.GetAttributes())
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{GetSimpleAttributeName(attr)}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate parameter attribute classes
            foreach (var attr in parameter.GetAttributes())
            {
                GenerateParameterAttributeClass(sb, typeSymbol, method, parameter, attr);
            }
        }

        private void GenerateParameterAttributeClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, IParameterSymbol parameter, AttributeData attribute)
        {
            var attrName = GetSimpleAttributeName(attribute);
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{attrName} : IStaticsAttributeInfo");
            sb.AppendLine("{");
            
            // Attribute type
            var attrTypeString = attribute.AttributeClass?.ToDisplayString();
            // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
            if (attrTypeString != null && attrTypeString.StartsWith("Statics."))
            {
                attrTypeString = "global::" + attrTypeString;
            }
            sb.AppendLine($"    public Type AttributeType => typeof({attrTypeString});");
            
            // Constructor arguments
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => [");
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{attrName}ConstructorArg{i}(),");
            }
            sb.AppendLine("    ];");
            
            // Named arguments
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => [");
            foreach (var namedArg in attribute.NamedArguments)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{attrName}NamedArg{namedArg.Key}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate constructor argument classes
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                var arg = attribute.ConstructorArguments[i];
                sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{attrName}ConstructorArg{i} : IStaticsAttributeArgument");
                sb.AppendLine("{");
                var argTypeString = arg.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                sb.AppendLine($"    public Type ArgumentType => typeof({argTypeString});");
                
                // Generate type-specific Value property
                var valueType = GetValuePropertyType(arg.Type);
                var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                sb.AppendLine($"    public {valueType} Value => {formattedValue};");
                
                sb.AppendLine("}");
                sb.AppendLine();
            }
            
            // Generate named argument classes
            foreach (var namedArg in attribute.NamedArguments)
            {
                sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{ToPascalCase(parameter.Name)}Attribute{attrName}NamedArg{namedArg.Key} : IStaticsAttributeNamedArgument");
                sb.AppendLine("{");
                sb.AppendLine($"    public string Name => \"{namedArg.Key}\";");
                var argTypeString = namedArg.Value.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && namedArg.Value.Type != null && !namedArg.Value.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
                if (argTypeString != null && argTypeString.StartsWith("Statics."))
                {
                    argTypeString = "global::" + argTypeString;
                }
                sb.AppendLine($"    public Type ArgumentType => typeof({argTypeString});");
                
                // Generate type-specific Value property
                var valueType = GetValuePropertyType(namedArg.Value.Type);
                var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                sb.AppendLine($"    public {valueType} Value => {formattedValue};");
                
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        private static IEnumerable<IMethodSymbol> GetStaticServiceMethods(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsStatic && m.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
                .Where(m => HasStaticsServiceMethodAttribute(m));
        }

        private static bool HasStaticsServiceMethodAttribute(IMethodSymbol method)
        {
            return method.GetAttributes().Any(a => 
                a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute");
        }

        private static string GetSimpleAttributeName(AttributeData attribute)
        {
            var fullName = attribute.AttributeClass?.Name ?? "Unknown";
            return fullName.EndsWith("Attribute") ? fullName.Substring(0, fullName.Length - 9) : fullName;
        }

        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        private static string FormatAttributeValue(object? value)
        {
            if (value == null)
                return "null";
            
            if (value is string str)
                return $"\"{str.Replace("\"", "\\\"")}\"";
            
            if (value is bool b)
                return b ? "true" : "false";
            
            if (value is char c)
                return $"'{c}'";
            
            return $"\"{value}\"";
        }

        private static string GetValuePropertyType(ITypeSymbol? type)
        {
            if (type == null)
                return "object?";
            
            var displayString = type.ToDisplayString();
            
            // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
            if (displayString.StartsWith("Statics."))
            {
                displayString = "global::" + displayString;
            }
            
            // For nullable value types, the display string already includes the ?
            if (type.IsValueType)
            {
                return displayString;
            }
            
            // For reference types, we need to handle nullable annotations
            // If it's already nullable (ends with ?), keep it
            // Otherwise, add ? for reference types
            if (!displayString.EndsWith("?"))
            {
                displayString += "?";
            }
            
            return displayString;
        }

        private static string FormatTypedAttributeValue(object? value, ITypeSymbol? type)
        {
            if (value == null)
                return "null";
            
            // Check for enums first, before numeric types
            // For enums, generate the full enum value reference with cast
            if (type?.TypeKind == TypeKind.Enum)
            {
                var enumType = type.ToDisplayString();
                // Remove nullable annotation for enum type reference
                if (enumType.EndsWith("?"))
                {
                    enumType = enumType.TrimEnd('?');
                }
                // Add global:: prefix if it's a Statics namespace enum to avoid namespace resolution issues
                if (enumType.StartsWith("Statics."))
                {
                    enumType = "global::" + enumType;
                }
                // Just return the numeric value cast to the enum type
                // This avoids namespace resolution issues
                return $"({enumType}){value}";
            }
            
            if (value is string str)
                return $"\"{str.Replace("\"", "\\\"")}\"";
            
            if (value is bool b)
                return b ? "true" : "false";
            
            if (value is char c)
                return $"'{c}'";
            
            if (value is byte || value is sbyte || 
                value is short || value is ushort || 
                value is int || value is uint || 
                value is long || value is ulong ||
                value is float || value is double || value is decimal)
                return value.ToString();
            
            // For Type values
            if (value is ITypeSymbol typeValue)
            {
                var typeString = typeValue.ToDisplayString();
                if (typeString.EndsWith("?") && !typeValue.IsValueType)
                {
                    typeString = typeString.TrimEnd('?');
                }
                return $"typeof({typeString})";
            }
            
            // Default to string representation
            return $"\"{value}\"";
        }
    }
    
    /// <summary>
    /// Configuration for Statics vendor generator
    /// </summary>
    public class StaticsConfig
    {
        public bool RequireBaseTypes { get; set; } = true;
        public bool IncludeParameterAttributes { get; set; } = true;
        public bool IncludeMethodAttributes { get; set; } = true;
    }
}