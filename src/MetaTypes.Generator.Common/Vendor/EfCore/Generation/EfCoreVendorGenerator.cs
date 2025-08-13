using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using MetaTypes.Generator.Common.Generator;

namespace MetaTypes.Generator.Common.Vendor.EfCore.Generation
{
    /// <summary>
    /// EfCore vendor generator that generates partial class extensions with EfCore-specific metadata
    /// </summary>
    public class EfCoreVendorGenerator : IVendorGenerator
    {
        private EfCoreConfig _config = new();
        
        public string VendorName => "EfCore";
        
        public string Description => "Generates EfCore-specific metadata extensions for entity types";
        
        /// <summary>
        /// Configure the EfCore vendor generator with its specific configuration
        /// </summary>
        public void Configure(JsonElement? config)
        {
            if (config.HasValue)
            {
                try
                {
                    _config = JsonSerializer.Deserialize<EfCoreConfig>(config.Value) ?? new EfCoreConfig();
                }
                catch
                {
                    // Use default config if parsing fails
                    _config = new EfCoreConfig();
                }
            }
            else
            {
                // Use default config when no config provided
                _config = new EfCoreConfig();
            }
        }

        /// <summary>
        /// Generates EfCore extensions only for types discovered by EfCore discovery methods
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
                // This prevents compilation errors when vendor extensions reference non-existent base classes
                
                yield break;
            }

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

            // Generate EfCore DI extension methods for the target namespace
            var diExtensionsSource = GenerateEfCoreServiceCollectionExtensions(context.TargetNamespace);
            yield return new GeneratedFile
            {
                FileName = $"EfCoreServiceCollectionExtensions.g.cs",
                Content = diExtensionsSource
            };
            
            // Generate EfCore extensions for each discovered entity type
            foreach (var entityType in efCoreTypes)
            {
                var source = GenerateEfCoreExtension(entityType);
                var assemblyName = entityType.ContainingAssembly.Name;
                yield return new GeneratedFile
                {
                    FileName = $"{assemblyName}_{entityType.Name}MetaTypeEfCore.g.cs",
                    Content = source
                };
            }
        }

        private string GenerateEfCoreExtension(INamedTypeSymbol typeSymbol)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.EfCore;");
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

        /// <summary>
        /// Generates EfCore-specific DI extension methods for the target namespace.
        /// </summary>
        private string GenerateEfCoreServiceCollectionExtensions(string targetNamespace)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.EfCore;");
            sb.AppendLine();
            sb.AppendLine($"namespace {targetNamespace};");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// EfCore vendor DI extension methods for MetaTypes generated in {targetNamespace} namespace.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class EfCoreServiceCollectionExtensions");
            sb.AppendLine("{");
            
            // Generate the EfCore-specific AddMetaTypes method
            var methodName = NamingUtils.ToAddVendorMetaTypesMethodName(targetNamespace, "EfCore");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Registers EfCore-specific MetaTypes from the {targetNamespace} namespace.");
            sb.AppendLine($"    /// This registers IMetaTypeEfCore interfaces for all EfCore entity types.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static IServiceCollection {methodName}(this IServiceCollection services)");
            sb.AppendLine("    {");
            sb.AppendLine($"        // First register the base MetaTypes");
            sb.AppendLine($"        services.{NamingUtils.ToAddMetaTypesMethodName(targetNamespace)}();");
            sb.AppendLine();
            sb.AppendLine("        // Register EfCore-specific interfaces");
            sb.AppendLine($"        foreach (var metaType in {targetNamespace}.MetaTypes.Instance.AssemblyMetaTypes)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (metaType is IMetaTypeEfCore efCoreType)");
            sb.AppendLine("            {");
            sb.AppendLine("                services.AddSingleton<IMetaTypeEfCore>(efCoreType);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// EfCore vendor service provider extension methods for retrieving registered MetaTypes.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class EfCoreServiceProviderExtensions");
            sb.AppendLine("{");
            
            // Add GetEfCoreMetaTypes method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets all registered EfCore MetaTypes from the service provider.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IEnumerable<IMetaTypeEfCore> GetEfCoreMetaTypes(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeEfCore>();");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add generic GetEfCoreMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific EfCore MetaType by entity type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeEfCore? GetEfCoreMetaType<T>(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeEfCore>()");
            sb.AppendLine("            .FirstOrDefault(mt => mt.ManagedType == typeof(T));");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add non-generic GetEfCoreMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific EfCore MetaType by entity type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeEfCore? GetEfCoreMetaType(this IServiceProvider serviceProvider, Type entityType)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeEfCore>()");
            sb.AppendLine("            .FirstOrDefault(mt => mt.ManagedType == entityType);");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Configuration for EfCore vendor generator
    /// </summary>
    public class EfCoreConfig
    {
        public bool RequireBaseTypes { get; set; } = true;
        public bool IncludeNavigationProperties { get; set; } = true;
        public bool IncludeForeignKeys { get; set; } = true;
    }
}