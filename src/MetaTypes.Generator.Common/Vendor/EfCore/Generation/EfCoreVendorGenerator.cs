using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using MetaTypes.Generator.Common.Generator;

namespace MetaTypes.Generator.Common.Vendor.EfCore.Generation
{
    /// <summary>
    /// EfCore vendor generator that generates partial class extensions with EfCore-specific metadata
    /// </summary>
    public class EfCoreVendorGenerator : IVendorGenerator
    {
        public string VendorName => "EfCore";
        
        public string Description => "Generates EfCore-specific metadata extensions for entity types";

        /// <summary>
        /// Generates EfCore extensions only for types discovered by EfCore discovery methods
        /// </summary>
        public IEnumerable<GeneratedFile> Generate(
            IEnumerable<DiscoveredType> discoveredTypes,
            Compilation compilation,
            GeneratorContext context)
        {
            // Filter to only types discovered by EfCore discovery methods
            var efCoreTypes = discoveredTypes
                .Where(dt => dt.WasDiscoveredByPrefix("EfCore."))
                .Select(dt => dt.TypeSymbol)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>()
                .ToList();

            if (!efCoreTypes.Any())
            {
                yield break;
            }

            // Generate EfCore extensions for each discovered entity type
            foreach (var entityType in efCoreTypes)
            {
                var source = GenerateEfCoreExtension(entityType);
                yield return new GeneratedFile
                {
                    FileName = $"{entityType.Name}EfCoreMetaType.g.cs",
                    Content = source
                };
            }
        }

        private string GenerateEfCoreExtension(INamedTypeSymbol typeSymbol)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine();
            
            // Use the assembly name for the MetaType namespace to match the base generator
            var assemblyName = typeSymbol.ContainingAssembly.Name;
            sb.AppendLine($"namespace {assemblyName};");
            sb.AppendLine();
            
            // Get properties for analysis
            var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .ToArray();
            
            // Generate partial class extension for the MetaType class with EfCore interface
            sb.AppendLine($"public partial class {typeSymbol.Name}MetaType : IMetaTypeEfCore");
            sb.AppendLine("{");
            
            // Get table name from [Table] attribute or derive from type name
            var tableName = GetTableName(typeSymbol);
            sb.AppendLine($"    public string? TableName => \"{tableName}\";");
            
            // Generate Keys collection
            var keyProperties = properties.Where(p => IsKeyProperty(p)).ToArray();
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IMetaTypeMemberEfCore> Keys => [");
            foreach (var keyProperty in keyProperties)
            {
                sb.AppendLine($"        {typeSymbol.Name}MetaTypeMember{keyProperty.Name}.Instance,");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate partial class extensions for each member with EfCore interface
            foreach (var property in properties)
            {
                sb.AppendLine($"public partial class {typeSymbol.Name}MetaTypeMember{property.Name} : IMetaTypeMemberEfCore");
                sb.AppendLine("{");
                
                // Check if this property is a primary key
                var isKey = IsKeyProperty(property);
                sb.AppendLine($"    public bool IsKey => {(isKey ? "true" : "false")};");
                
                // Check if this property is a foreign key
                var isForeignKey = IsForeignKeyProperty(property);
                sb.AppendLine($"    public bool IsForeignKey => {(isForeignKey ? "true" : "false")};");
                
                // Check if this property is marked as not mapped
                var isNotMapped = IsNotMappedProperty(property);
                sb.AppendLine($"    public bool IsNotMapped => {(isNotMapped ? "true" : "false")};");
                
                // For now, ForeignKeyMember is null - could be enhanced later
                sb.AppendLine($"    public IMetaTypeMember? ForeignKeyMember => null;");
                
                sb.AppendLine("}");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private static string GetTableName(INamedTypeSymbol typeSymbol)
        {
            // Check for [Table] attribute
            var tableAttribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "TableAttribute" || 
                                    a.AttributeClass?.Name == "Table");
            
            if (tableAttribute != null && tableAttribute.ConstructorArguments.Length > 0)
            {
                var arg = tableAttribute.ConstructorArguments[0];
                if (arg.Value is string tableName)
                {
                    return tableName;
                }
            }
            
            // Default to pluralized type name
            return typeSymbol.Name.EndsWith("s") ? typeSymbol.Name + "es" : 
                   typeSymbol.Name.EndsWith("y") ? typeSymbol.Name.Substring(0, typeSymbol.Name.Length - 1) + "ies" :
                   typeSymbol.Name + "s";
        }

        private static bool IsKeyProperty(IPropertySymbol property)
        {
            // Check for [Key] attribute
            if (property.GetAttributes().Any(a => 
                a.AttributeClass?.Name == "KeyAttribute" || 
                a.AttributeClass?.Name == "Key"))
            {
                return true;
            }
            
            // Convention: Property named "Id" or "{TypeName}Id"
            var propertyName = property.Name;
            if (propertyName.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            var typeName = property.ContainingType.Name;
            if (propertyName.Equals($"{typeName}Id", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            return false;
        }

        private static bool IsForeignKeyProperty(IPropertySymbol property)
        {
            // Check for [ForeignKey] attribute
            return property.GetAttributes().Any(a => 
                a.AttributeClass?.Name == "ForeignKeyAttribute" || 
                a.AttributeClass?.Name == "ForeignKey");
        }

        private static bool IsNotMappedProperty(IPropertySymbol property)
        {
            // Check for [NotMapped] attribute
            return property.GetAttributes().Any(a => 
                a.AttributeClass?.Name == "NotMappedAttribute" || 
                a.AttributeClass?.Name == "NotMapped");
        }
    }
}