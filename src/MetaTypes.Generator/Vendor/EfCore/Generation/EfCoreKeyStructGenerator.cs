using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Common.Vendor.EfCore.Generation;

/// <summary>
/// Generates strongly-typed key structs for EfCore entities.
/// </summary>
public static class EfCoreKeyStructGenerator
{
    /// <summary>
    /// Generates a key struct for an entity type with the specified key properties.
    /// </summary>
    /// <param name="entitySymbol">The entity type symbol</param>
    /// <param name="keyProperties">The key properties in order</param>
    /// <returns>The generated struct source code</returns>
    public static string GenerateKeyStruct(INamedTypeSymbol entitySymbol, IPropertySymbol[] keyProperties)
    {
        var sb = new StringBuilder();
        var entityNamespace = entitySymbol.ContainingNamespace.ToDisplayString();
        var entityTypeName = entitySymbol.ToDisplayString();
        var keyStructName = $"{entitySymbol.Name}Key";

        // Using statements
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Linq.Expressions;");
        sb.AppendLine("using MetaTypes.Abstractions.Vendor.EfCore;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine($"namespace {entityNamespace};");
        sb.AppendLine();

        // Struct declaration
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Strongly-typed key for {entitySymbol.Name} entity.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public readonly struct {keyStructName} : IEntityKey<{entityTypeName}>, IEquatable<{keyStructName}>");
        sb.AppendLine("{");

        // Key properties
        foreach (var keyProperty in keyProperties)
        {
            var propertyTypeString = keyProperty.Type.ToDisplayString();
            sb.AppendLine($"    public {propertyTypeString} {keyProperty.Name} {{ get; }}");
        }
        sb.AppendLine();

        // Constructor from scalar values
        sb.Append($"    public {keyStructName}(");
        sb.Append(string.Join(", ", keyProperties.Select(kp =>
            $"{kp.Type.ToDisplayString()} {ToCamelCase(kp.Name)}")));
        sb.AppendLine(")");
        sb.AppendLine("    {");
        foreach (var keyProperty in keyProperties)
        {
            sb.AppendLine($"        {keyProperty.Name} = {ToCamelCase(keyProperty.Name)};");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // Constructor from entity instance
        sb.AppendLine($"    public {keyStructName}({entityTypeName} entity)");
        sb.AppendLine("    {");
        foreach (var keyProperty in keyProperties)
        {
            sb.AppendLine($"        {keyProperty.Name} = entity.{keyProperty.Name};");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // Where expression property
        sb.AppendLine($"    public Expression<Func<{entityTypeName}, bool>> Where");
        sb.AppendLine("    {");
        sb.AppendLine("        get");
        sb.AppendLine("        {");

        // Capture values in local variables (required for readonly struct)
        foreach (var keyProp in keyProperties)
        {
            sb.AppendLine($"            var {ToCamelCase(keyProp.Name)} = {keyProp.Name};");
        }

        sb.Append("            return entity => ");
        if (keyProperties.Length == 1)
        {
            var keyProp = keyProperties[0];
            sb.AppendLine($"entity.{keyProp.Name} == {ToCamelCase(keyProp.Name)};");
        }
        else
        {
            // Build composite key expression: entity => entity.Key1 == key1 && entity.Key2 == key2 && ...
            for (int i = 0; i < keyProperties.Length; i++)
            {
                if (i > 0)
                    sb.Append(" && ");
                var keyProp = keyProperties[i];
                sb.Append($"entity.{keyProp.Name} == {ToCamelCase(keyProp.Name)}");
            }
            sb.AppendLine(";");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ToString override
        sb.Append($"    public override string ToString() => ");
        if (keyProperties.Length == 1)
        {
            var keyProp = keyProperties[0];
            sb.AppendLine($"$\"{keyStructName}({keyProp.Name}={{{keyProp.Name}}})\" ;");
        }
        else
        {
            sb.Append("$\"");
            sb.Append($"{keyStructName}(");
            sb.Append(string.Join(", ", keyProperties.Select(kp => $"{kp.Name}={{{kp.Name}}}")));
            sb.AppendLine(")\";");
        }
        sb.AppendLine();

        // Equals method
        sb.AppendLine($"    public bool Equals({keyStructName} other) =>");
        if (keyProperties.Length == 1)
        {
            var keyProp = keyProperties[0];
            sb.AppendLine($"        {keyProp.Name} == other.{keyProp.Name};");
        }
        else
        {
            sb.Append("        ");
            for (int i = 0; i < keyProperties.Length; i++)
            {
                if (i > 0)
                    sb.Append(" && ");
                var keyProp = keyProperties[i];
                sb.Append($"{keyProp.Name} == other.{keyProp.Name}");
            }
            sb.AppendLine(";");
        }
        sb.AppendLine();

        // Override Equals
        sb.AppendLine($"    public override bool Equals(object? obj) =>");
        sb.AppendLine($"        obj is {keyStructName} other && Equals(other);");
        sb.AppendLine();

        // GetHashCode
        sb.Append("    public override int GetHashCode()");
        if (keyProperties.Length == 1)
        {
            var keyProp = keyProperties[0];
            sb.AppendLine($" => {keyProp.Name}.GetHashCode();");
        }
        else if (keyProperties.Length <= 8)
        {
            // Use HashCode.Combine for 2-8 keys
            sb.AppendLine(" =>");
            sb.Append($"        HashCode.Combine(");
            sb.Append(string.Join(", ", keyProperties.Select(kp => kp.Name)));
            sb.AppendLine(");");
        }
        else
        {
            // Use HashCode builder for 9+ keys
            sb.AppendLine();
            sb.AppendLine("    {");
            sb.AppendLine("        var hash = new HashCode();");
            foreach (var keyProp in keyProperties)
            {
                sb.AppendLine($"        hash.Add({keyProp.Name});");
            }
            sb.AppendLine("        return hash.ToHashCode();");
            sb.AppendLine("    }");
        }
        sb.AppendLine();

        // Equality operators
        sb.AppendLine($"    public static bool operator ==({keyStructName} left, {keyStructName} right) =>");
        sb.AppendLine("        left.Equals(right);");
        sb.AppendLine();
        sb.AppendLine($"    public static bool operator !=({keyStructName} left, {keyStructName} right) =>");
        sb.AppendLine("        !left.Equals(right);");

        // Close struct
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;
        return char.ToLower(name[0]) + name.Substring(1);
    }
}
