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

            // Generate Statics DI extension methods for the target namespace
            var diExtensionsSource = GenerateStaticsServiceCollectionExtensions(context.TargetNamespace);
            yield return new GeneratedFile
            {
                FileName = $"StaticsServiceCollectionExtensions.g.cs",
                Content = diExtensionsSource
            };
            
            // Generate Statics extensions for each discovered static class
            var isFirstFile = true;
            foreach (var staticClass in staticsTypes)
            {
                var source = GenerateStaticsExtension(staticClass, includeSharedClasses: isFirstFile);
                var assemblyName = staticClass.ContainingAssembly.Name;
                yield return new GeneratedFile
                {
                    FileName = $"{assemblyName}_{staticClass.Name}MetaTypeStatics.g.cs",
                    Content = source
                };
                isFirstFile = false; // Only include shared classes in the first file
            }
        }

        private string GenerateStaticsExtension(INamedTypeSymbol typeSymbol, bool includeSharedClasses = false)
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

            // Generate shared wrapper classes only in the first file - OPTIMIZATION: Replaces explosion of individual classes
            if (includeSharedClasses)
            {
                GenerateSharedWrapperClasses(sb);
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
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{NamingUtils.ToPascalCase(param.Name)}(),");
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
            
            // Static readonly data for constructor arguments - OPTIMIZATION: Reduce allocation
            sb.AppendLine();
            sb.AppendLine("    private static readonly (Type Type, object? Value)[] _constructorArgs = [");
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                var arg = attribute.ConstructorArguments[i];
                var argTypeString = arg.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                sb.AppendLine($"        (typeof({argTypeString}), {formattedValue}),");
            }
            sb.AppendLine("    ];");
            
            // Static readonly data for named arguments - OPTIMIZATION: Reduce allocation
            sb.AppendLine();
            sb.AppendLine("    private static readonly (string Name, Type Type, object? Value)[] _namedArgs = [");
            foreach (var namedArg in attribute.NamedArguments)
            {
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
                var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                sb.AppendLine($"        (\"{namedArg.Key}\", typeof({argTypeString}), {formattedValue}),");
            }
            sb.AppendLine("    ];");
            
            // Lazy-initialized collections - OPTIMIZATION: Reduce allocations
            sb.AppendLine();
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeArgument>? _constructorArguments;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeNamedArgument>? _namedArguments;");
            
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => ");
            sb.AppendLine("        _constructorArguments ??= _constructorArgs");
            sb.AppendLine("            .Select((arg, i) => new StaticsAttributeArgument(arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => ");
            sb.AppendLine("        _namedArguments ??= _namedArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeNamedArgument(arg.Name, arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine("}");
            sb.AppendLine();
        }

        private void GenerateParameterClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, IParameterSymbol parameter)
        {
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{NamingUtils.ToPascalCase(parameter.Name)} : IStaticsParameterInfo");
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
            
            // Static readonly data for parameter attributes - OPTIMIZATION: Similar to attribute optimization
            sb.AppendLine();
            sb.AppendLine("    private static readonly StaticsAttributeData[] _attributeData = [");
            foreach (var attr in parameter.GetAttributes())
            {
                var attrTypeString = attr.AttributeClass?.ToDisplayString();
                if (attrTypeString != null && attrTypeString.StartsWith("Statics."))
                {
                    attrTypeString = "global::" + attrTypeString;
                }
                
                sb.AppendLine($"        new StaticsAttributeData(typeof({attrTypeString}),");
                
                // Constructor arguments
                sb.AppendLine("            [");
                for (int i = 0; i < attr.ConstructorArguments.Length; i++)
                {
                    var arg = attr.ConstructorArguments[i];
                    var argTypeString = arg.Type?.ToDisplayString();
                    if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                    {
                        argTypeString = argTypeString.TrimEnd('?');
                    }
                    var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                    sb.AppendLine($"                (typeof({argTypeString}), {formattedValue}),");
                }
                sb.AppendLine("            ],");
                
                // Named arguments
                sb.AppendLine("            [");
                foreach (var namedArg in attr.NamedArguments)
                {
                    var argTypeString = namedArg.Value.Type?.ToDisplayString();
                    if (argTypeString != null && argTypeString.EndsWith("?") && namedArg.Value.Type != null && !namedArg.Value.Type.IsValueType)
                    {
                        argTypeString = argTypeString.TrimEnd('?');
                    }
                    if (argTypeString != null && argTypeString.StartsWith("Statics."))
                    {
                        argTypeString = "global::" + argTypeString;
                    }
                    var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                    sb.AppendLine($"                (\"{namedArg.Key}\", typeof({argTypeString}), {formattedValue}),");
                }
                sb.AppendLine("            ]),");
            }
            sb.AppendLine("    ];");
            
            // Lazy-initialized parameter attributes collection - OPTIMIZATION
            sb.AppendLine();
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeInfo>? _parameterAttributes;");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes => ");
            sb.AppendLine("        _parameterAttributes ??= _attributeData");
            sb.AppendLine("            .Select(data => new StaticsAttributeInfo(data.AttributeType, data.ConstructorArgs, data.NamedArgs))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine("}");
            sb.AppendLine();
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

        /// <summary>
        /// Generates Statics-specific DI extension methods for the target namespace.
        /// </summary>
        private string GenerateStaticsServiceCollectionExtensions(string targetNamespace)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine();
            sb.AppendLine($"namespace {targetNamespace};");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Statics vendor DI extension methods for MetaTypes generated in {targetNamespace} namespace.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class StaticsServiceCollectionExtensions");
            sb.AppendLine("{");
            
            // Generate the Statics-specific AddMetaTypes method
            var methodName = NamingUtils.ToAddVendorMetaTypesMethodName(targetNamespace, "Statics");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Registers Statics-specific MetaTypes from the {targetNamespace} namespace.");
            sb.AppendLine($"    /// This registers IMetaTypeStatics interfaces for all static service classes.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static IServiceCollection {methodName}(this IServiceCollection services)");
            sb.AppendLine("    {");
            sb.AppendLine($"        // First register the base MetaTypes");
            sb.AppendLine($"        services.{NamingUtils.ToAddMetaTypesMethodName(targetNamespace)}();");
            sb.AppendLine();
            sb.AppendLine("        // Register Statics-specific interfaces");
            sb.AppendLine($"        foreach (var metaType in {targetNamespace}.MetaTypes.Instance.AssemblyMetaTypes)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (metaType is IMetaTypeStatics staticsType)");
            sb.AppendLine("            {");
            sb.AppendLine("                services.AddSingleton<IMetaTypeStatics>(staticsType);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Statics vendor service provider extension methods for retrieving registered MetaTypes.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class StaticsServiceProviderExtensions");
            sb.AppendLine("{");
            
            // Add GetStaticsMetaTypes method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets all registered Statics MetaTypes from the service provider.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IEnumerable<IMetaTypeStatics> GetStaticsMetaTypes(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>();");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add generic GetStaticsMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific Statics MetaType by static service class type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeStatics? GetStaticsMetaType<T>(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>()");
            sb.AppendLine("            .FirstOrDefault(mt => ((IMetaType)mt).ManagedType == typeof(T));");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add non-generic GetStaticsMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific Statics MetaType by static service class type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeStatics? GetStaticsMetaType(this IServiceProvider serviceProvider, Type serviceType)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>()");
            sb.AppendLine("            .FirstOrDefault(mt => ((IMetaType)mt).ManagedType == serviceType);");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates shared wrapper classes that replace the explosion of individual argument classes.
        /// OPTIMIZATION: Reduces class count by ~80% for attribute arguments.
        /// </summary>
        private void GenerateSharedWrapperClasses(StringBuilder sb)
        {
            sb.AppendLine("#region Shared Wrapper Classes - OPTIMIZATION");
            sb.AppendLine();
            
            // Data structure for consolidated attribute data
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Data structure for consolidated attribute information - replaces individual attribute classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public readonly record struct StaticsAttributeData(");
            sb.AppendLine("    Type AttributeType,");
            sb.AppendLine("    (Type Type, object? Value)[] ConstructorArgs,");
            sb.AppendLine("    (string Name, Type Type, object? Value)[] NamedArgs);");
            sb.AppendLine();
            
            // Generic attribute info wrapper
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for attribute info - replaces individual attribute info classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeInfo : IStaticsAttributeInfo");
            sb.AppendLine("{");
            sb.AppendLine("    public Type AttributeType { get; }");
            sb.AppendLine("    private readonly (Type Type, object? Value)[] _constructorArgs;");
            sb.AppendLine("    private readonly (string Name, Type Type, object? Value)[] _namedArgs;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeArgument>? _constructorArguments;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeNamedArgument>? _namedArguments;");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeInfo(Type attributeType, (Type Type, object? Value)[] constructorArgs, (string Name, Type Type, object? Value)[] namedArgs)");
            sb.AppendLine("    {");
            sb.AppendLine("        AttributeType = attributeType;");
            sb.AppendLine("        _constructorArgs = constructorArgs;");
            sb.AppendLine("        _namedArgs = namedArgs;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => ");
            sb.AppendLine("        _constructorArguments ??= _constructorArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeArgument(arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => ");
            sb.AppendLine("        _namedArguments ??= _namedArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeNamedArgument(arg.Name, arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generic attribute argument wrapper
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for attribute constructor arguments - replaces individual argument classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeArgument : IStaticsAttributeArgument");
            sb.AppendLine("{");
            sb.AppendLine("    public Type ArgumentType { get; }");
            sb.AppendLine("    public object? Value { get; }");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeArgument(Type argumentType, object? value)");
            sb.AppendLine("    {");
            sb.AppendLine("        ArgumentType = argumentType;");
            sb.AppendLine("        Value = value;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generic named attribute argument wrapper  
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for named attribute arguments - replaces individual named argument classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeNamedArgument : IStaticsAttributeNamedArgument");
            sb.AppendLine("{");
            sb.AppendLine("    public string Name { get; }");
            sb.AppendLine("    public Type ArgumentType { get; }");
            sb.AppendLine("    public object? Value { get; }");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeNamedArgument(string name, Type argumentType, object? value)");
            sb.AppendLine("    {");
            sb.AppendLine("        Name = name;");
            sb.AppendLine("        ArgumentType = argumentType;");
            sb.AppendLine("        Value = value;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("#endregion");
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