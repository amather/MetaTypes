using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Shared logic for generating core MetaType classes and provider.
/// Used by both the main generator and extension generators to ensure 
/// core files are always available.
/// </summary>
public static class CoreMetaTypeGenerator
{
    /// <summary>
    /// Generates the MetaTypes provider class that implements IMetaTypeProvider.
    /// </summary>
    public static string GenerateMetaTypesProvider(string assemblyNamespace, IEnumerable<INamedTypeSymbol> typeSymbols)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using MetaTypes.Abstractions;");
        sb.AppendLine();
        sb.AppendLine($"namespace {assemblyNamespace};");
        sb.AppendLine();
        sb.AppendLine("public partial class MetaTypes : IMetaTypeProvider");
        sb.AppendLine("{");
        sb.AppendLine("    private static MetaTypes? _instance;");
        sb.AppendLine("    public static MetaTypes Instance => _instance ??= new();");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyList<IMetaType> AssemblyMetaTypes => [");
        
        foreach (var typeSymbol in typeSymbols)
        {
            sb.AppendLine($"        {typeSymbol.Name}MetaType.Instance,");
        }
        
        sb.AppendLine("    ];");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a complete MetaType class for the given type symbol.
    /// </summary>
    public static string GenerateMetaTypeClass(INamedTypeSymbol typeSymbol, string assemblyNamespace, IList<INamedTypeSymbol>? discoveredTypes = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Linq.Expressions;");
        sb.AppendLine("using MetaTypes.Abstractions;");
        sb.AppendLine();
        sb.AppendLine($"namespace {assemblyNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {typeSymbol.Name}MetaType : IMetaType, IMetaType<{typeSymbol.ToDisplayString()}>");
        sb.AppendLine("{");
        sb.AppendLine($"    private static {typeSymbol.Name}MetaType? _instance;");
        sb.AppendLine($"    public static {typeSymbol.Name}MetaType Instance => _instance ??= new();");
        sb.AppendLine();
        
        // Basic properties
        sb.AppendLine($"    public Type ManagedType => typeof({typeSymbol.ToDisplayString()});");
        sb.AppendLine($"    public string ManagedTypeName => \"{typeSymbol.Name}\";");
        sb.AppendLine($"    public string ManagedTypeNamespace => \"{typeSymbol.ContainingNamespace.ToDisplayString()}\";");
        sb.AppendLine($"    public string ManagedTypeAssembly => \"{typeSymbol.ContainingAssembly.Name}\";");
        sb.AppendLine($"    public string ManagedTypeFullName => \"{typeSymbol.ToDisplayString()}\";");

        // Generic type arguments
        if (typeSymbol.IsGenericType)
        {
            sb.AppendLine("    public Type[]? GenericTypeArguments => [");
            foreach (var typeArg in typeSymbol.TypeArguments)
            {
                sb.AppendLine($"        typeof({typeArg.ToDisplayString()}),");
            }
            sb.AppendLine("    ];");
        }
        else
        {
            sb.AppendLine("    public Type[]? GenericTypeArguments => null;");
        }

        // Attributes (skip complex attributes like TableAttribute for now)
        var attributes = typeSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.Name != "MetaTypeAttribute" && a.AttributeClass?.Name != "TableAttribute")
            .ToArray();
        if (attributes.Length > 0)
        {
            sb.AppendLine("    public Attribute[]? Attributes => [");
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass != null && attr.ConstructorArguments.Length == 0)
                {
                    sb.AppendLine($"        new {attr.AttributeClass.ToDisplayString()}(),");
                }
            }
            sb.AppendLine("    ];");
        }
        else
        {
            sb.AppendLine("    public Attribute[]? Attributes => null;");
        }

        // Members
        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .ToArray();

        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyList<IMetaTypeMember> Members => [");
        foreach (var property in properties)
        {
            sb.AppendLine($"        {typeSymbol.Name}MetaTypeMember{property.Name}.Instance,");
        }
        sb.AppendLine("    ];");

        // Member finding methods
        sb.AppendLine();
        sb.AppendLine("    public IMetaTypeMember? FindMember(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        return Members.FirstOrDefault(m => m.MemberName == name);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public IMetaTypeMember FindRequiredMember(string name)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return FindMember(name) ?? throw new InvalidOperationException($\"Member '{{name}}' not found on type '{typeSymbol.ToDisplayString()}'\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public IMetaTypeMember? FindMember<TProperty>(Expression<Func<{typeSymbol.ToDisplayString()}, TProperty>> expression)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (expression.Body is MemberExpression memberExpr)");
        sb.AppendLine("        {");
        sb.AppendLine("            return FindMember(memberExpr.Member.Name);");
        sb.AppendLine("        }");
        sb.AppendLine("        throw new ArgumentException(\"Expression must be a member access\", nameof(expression));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public IMetaTypeMember FindRequiredMember<TProperty>(Expression<Func<{typeSymbol.ToDisplayString()}, TProperty>> expression)");
        sb.AppendLine("    {");
        sb.AppendLine("        return FindMember(expression) ?? throw new InvalidOperationException($\"Member not found for expression\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Generate member classes
        sb.AppendLine();
        foreach (var property in properties)
        {
            sb.AppendLine(GenerateMetaTypeMemberClass(typeSymbol, property, discoveredTypes));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a MetaTypeMember class for a specific property.
    /// </summary>
    private static string GenerateMetaTypeMemberClass(INamedTypeSymbol typeSymbol, IPropertySymbol property, IList<INamedTypeSymbol>? discoveredTypes)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"public partial class {typeSymbol.Name}MetaTypeMember{property.Name} : IMetaTypeMember");
        sb.AppendLine("{");
        sb.AppendLine($"    private static {typeSymbol.Name}MetaTypeMember{property.Name}? _instance;");
        sb.AppendLine($"    public static {typeSymbol.Name}MetaTypeMember{property.Name} Instance => _instance ??= new();");
        sb.AppendLine();
        sb.AppendLine($"    public string MemberName => \"{property.Name}\";");
        // Handle nullable reference types properly for typeof
        var typeString = property.Type.ToDisplayString();
        if (typeString.EndsWith("?") && !property.Type.IsValueType)
        {
            // Remove nullable annotation for typeof since it can't handle nullable reference types
            typeString = typeString.TrimEnd('?');
        }
        sb.AppendLine($"    public Type MemberType => typeof({typeString});");
        sb.AppendLine($"    public bool HasSetter => {(property.SetMethod != null ? "true" : "false")};");

        // Check if it's a list/collection type
        var isList = property.Type is INamedTypeSymbol namedType && 
                     (namedType.ConstructedFrom.ToDisplayString().StartsWith("System.Collections.Generic.List<") ||
                      namedType.ConstructedFrom.ToDisplayString().StartsWith("System.Collections.Generic.IList<") ||
                      namedType.ConstructedFrom.ToDisplayString().StartsWith("System.Collections.Generic.ICollection<") ||
                      namedType.ConstructedFrom.ToDisplayString().StartsWith("System.Collections.Generic.IEnumerable<"));

        sb.AppendLine($"    public bool IsList => {(isList ? "true" : "false")};");

        // Generic type arguments for lists
        if (isList && property.Type is INamedTypeSymbol listType && listType.TypeArguments.Length > 0)
        {
            sb.AppendLine("    public Type[]? GenericTypeArguments => [");
            foreach (var typeArg in listType.TypeArguments)
            {
                sb.AppendLine($"        typeof({typeArg.ToDisplayString()}),");
            }
            sb.AppendLine("    ];");
        }
        else
        {
            sb.AppendLine("    public Type[]? GenericTypeArguments => null;");
        }

        // Property attributes
        var attributes = property.GetAttributes()
            .Where(a => a.AttributeClass?.Name != "MetaTypeAttribute")
            .ToArray();
        if (attributes.Length > 0)
        {
            sb.AppendLine("    public Attribute[]? Attributes => [");
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass != null && attr.ConstructorArguments.Length == 0)
                {
                    sb.AppendLine($"        new {attr.AttributeClass.ToDisplayString()}(),");
                }
            }
            sb.AppendLine("    ];");
        }
        else
        {
            sb.AppendLine("    public Attribute[]? Attributes => null;");
        }

        // Cross-reference detection: check if property type has a MetaType
        var (isMetaType, metaTypeReference) = DetectMetaTypeCrossReference(property, discoveredTypes);
        sb.AppendLine($"    public bool IsMetaType => {(isMetaType ? "true" : "false")};");
        sb.AppendLine($"    public IMetaType? MetaType => {(metaTypeReference != null ? metaTypeReference : "null")};");
        
        // Dynamic property access methods
        sb.AppendLine();
        sb.AppendLine("    public object? GetValue(object obj)");
        sb.AppendLine("    {");
        sb.AppendLine($"        if (obj is {typeSymbol.ToDisplayString()} typedObj)");
        sb.AppendLine($"            return typedObj.{property.Name};");
        sb.AppendLine($"        throw new ArgumentException($\"Object must be of type {typeSymbol.ToDisplayString()}\", nameof(obj));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public void SetValue(object obj, object? value)");
        sb.AppendLine("    {");
        if (property.SetMethod != null)
        {
            // Check if the property is init-only
            var isInitOnly = property.SetMethod.IsInitOnly;
            if (isInitOnly)
            {
                sb.AppendLine($"        throw new InvalidOperationException(\"Property '{property.Name}' is init-only and cannot be set after object initialization.\");");
            }
            else
            {
                sb.AppendLine($"        if (obj is {typeSymbol.ToDisplayString()} typedObj)");
                sb.AppendLine("        {");
                sb.AppendLine($"            typedObj.{property.Name} = ({property.Type.ToDisplayString()})value!;");
                sb.AppendLine("            return;");
                sb.AppendLine("        }");
                sb.AppendLine($"        throw new ArgumentException($\"Object must be of type {typeSymbol.ToDisplayString()}\", nameof(obj));");
            }
        }
        else
        {
            sb.AppendLine($"        throw new InvalidOperationException(\"Property '{property.Name}' is read-only and cannot be set.\");");
        }
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
    
    /// <summary>
    /// Detects if a property type has a corresponding MetaType among the discovered types
    /// </summary>
    private static (bool isMetaType, string? metaTypeReference) DetectMetaTypeCrossReference(IPropertySymbol property, IList<INamedTypeSymbol>? discoveredTypes)
    {
        if (discoveredTypes == null || discoveredTypes.Count == 0)
            return (false, null);
            
        var propertyType = property.Type;
        INamedTypeSymbol? typeToCheck = null;
        
        if (propertyType is INamedTypeSymbol namedType)
        {
            // Handle different scenarios:
            // 1. Direct type reference (e.g., Customer, Customer?)
            // 2. Generic collections (e.g., List<CustomerAddress>, ICollection<T>)
            // 3. Nullable value types (e.g., int?)
            
            // Check if it's a generic type (like List<T>, ICollection<T>, etc.)
            if (namedType.IsGenericType && namedType.TypeArguments.Length > 0)
            {
                // For generic collections, check the first type argument
                if (namedType.TypeArguments[0] is INamedTypeSymbol genericArg)
                {
                    typeToCheck = genericArg;
                }
            }
            // Handle nullable value types
            else if (namedType.IsValueType && namedType.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T 
                     && namedType.TypeArguments.Length > 0)
            {
                if (namedType.TypeArguments[0] is INamedTypeSymbol nullableArg)
                {
                    typeToCheck = nullableArg;
                }
            }
            // Direct type reference
            else if (namedType.CanBeReferencedByName)
            {
                typeToCheck = namedType;
            }
        }
        
        if (typeToCheck != null)
        {
            // Check if this type is in our discovered types list
            var matchingType = discoveredTypes.FirstOrDefault(dt => 
                SymbolEqualityComparer.Default.Equals(dt, typeToCheck));
                
            if (matchingType != null)
            {
                // Generate the MetaType reference - use the assembly name as namespace
                var metaTypeNamespace = matchingType.ContainingAssembly.Name;
                return (true, $"{metaTypeNamespace}.{matchingType.Name}MetaType.Instance");
            }
        }
        
        return (false, null);
    }
}