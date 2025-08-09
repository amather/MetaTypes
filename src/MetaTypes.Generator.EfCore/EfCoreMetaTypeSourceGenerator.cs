using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using MetaTypes.Generator.Common;
using MetaTypes.Generator.EfCore.Common;

namespace MetaTypes.Generator.EfCore;

[Generator]
public class EfCoreMetaTypeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get configuration from AdditionalFiles (JSON configuration)
        var configuration = context.AdditionalTextsProvider
            .Collect()
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((combined, _) => 
            {
                var (additionalFiles, configProvider) = combined;
                var fullConfig = ConfigurationLoader.LoadFromAdditionalFiles(additionalFiles, configProvider);
                return fullConfig.EfCoreGenerator!;
            });

        // Combine compilation with configuration
        var compilationAndConfig = context.CompilationProvider.Combine(configuration);

        context.RegisterSourceOutput(compilationAndConfig,
            (spc, source) => 
            {
                var compilation = source.Left;
                var config = source.Right;
                
                // Check orchestrated configuration
                if (!ShouldRunGenerator(config))
                {
                    // Add diagnostic explaining why generator was skipped
                    spc.AddSource("_EfCoreGeneratorSkipped.g.cs", $@"
// EfCore MetaType generation skipped by configuration
// EnableBaseDetection (legacy): {config.EnableBaseDetection}
// EfCore.RequireBaseTypes: {config.EfCore?.RequireBaseTypes ?? true}
// Configure EnableBaseDetection = true to run this generator
");
                    return;
                }
                
                Execute(compilation, config, spc);
            });
    }

    /// <summary>
    /// Determines if the EfCore generator should run based on orchestrated configuration
    /// </summary>
    private static bool ShouldRunGenerator(EfCoreGeneratorOptions config)
    {
        // Legacy compatibility: respect EnableBaseDetection
        if (!config.EnableBaseDetection) return false;
        
        // Orchestrated configuration: respect EfCore.RequireBaseTypes
        return config.EfCore?.RequireBaseTypes ?? true;
    }

    private static void Execute(Compilation compilation, EfCoreGeneratorOptions config, SourceProductionContext context)
    {
        // Register EfCore discovery methods for orchestrated discovery
        EfCoreDiscoveryMethods.RegisterWithUnifiedDiscovery();
        
        // Use orchestrated discovery system - this will use ALL configured discovery methods
        // including common methods (MetaTypesAttributes, MetaTypesReferences) and EfCore methods
        var discoveredTypes = UnifiedTypeDiscovery.DiscoverTypes(compilation, config.Discovery);
        
        var hasEfCoreConfig = config.Discovery.Methods is EfCoreDiscoveryMethodsConfig;
        var efCoreMethodsConfig = config.Discovery.Methods as EfCoreDiscoveryMethodsConfig;
        
        // Add diagnostic file if enabled
        if (config.EnableDiagnosticFiles)
        {
            var discoveryStats = discoveredTypes
                .GroupBy(dt => $"{dt.Source}-{dt.DiscoveredBy}")
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();
            
            // Check for DbContexts in compilation
            var dbContexts = new List<string>();
            var dbSetDetails = new List<string>();
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (semanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
                    {
                        var current = classSymbol.BaseType;
                        while (current != null)
                        {
                            if (current.Name == "DbContext")
                            {
                                dbContexts.Add($"{classSymbol.Name} (Base: {current.ToDisplayString()})" );
                                
                                // Check DbSet properties
                                foreach (var member in classSymbol.GetMembers())
                                {
                                    if (member is IPropertySymbol property)
                                    {
                                        var propType = property.Type as INamedTypeSymbol;
                                        var isGeneric = propType?.IsGenericType ?? false;
                                        var typeName = propType?.Name ?? "null";
                                        var typeArgs = propType?.TypeArguments.Length ?? 0;
                                        dbSetDetails.Add($"{property.Name}: {property.Type.ToDisplayString()} (IsGeneric:{isGeneric}, Name:{typeName}, Args:{typeArgs})");
                                    }
                                }
                                break;
                            }
                            current = current.BaseType;
                        }
                    }
                }
            }
                
            context.AddSource("_EfCoreGeneratorDiagnostic.g.cs", $@"
// Generated by EfCore generator at {System.DateTime.Now}
// Assembly: {compilation.Assembly.Name}
// ConfiguredAssemblyName: {config.AssemblyName}
// Base Detection Enabled: {config.EnableBaseDetection}
// Diagnostic Files Enabled: {config.EnableDiagnosticFiles}
// Config Keys Found: {config.DebugInfo}
// Discovery Methods: EfCore only - HasConfig:{hasEfCoreConfig}, EfCoreEntities:{efCoreMethodsConfig?.EfCoreEntities}, DbContextScanning:{efCoreMethodsConfig?.DbContextScanning}
// Discovered types: {discoveredTypes.Count}
// Discovery breakdown: {string.Join(", ", discoveryStats)}
// Types: {string.Join(", ", discoveredTypes.Select(dt => $"{dt.TypeSymbol.Name} ({dt.Source} by {dt.DiscoveredBy})"))}
// DbContexts found: {dbContexts.Count} - {string.Join(", ", dbContexts)}
// DbSet properties: {string.Join(", ", dbSetDetails)}
// Syntax trees count: {compilation.SyntaxTrees.Count()}
");
        }

        // Process all discovered types - we generate EfCore extensions for any entity type
        // that was discovered (regardless of discovery method)
        var efCoreEntityTypes = discoveredTypes
            .Select(dt => dt.TypeSymbol)
            .ToList();

        // Create a HashSet for fast lookups using netstandard2.0 compatible approach
        var entityTypeSet = new HashSet<INamedTypeSymbol>(efCoreEntityTypes, SymbolEqualityComparer.Default);

        // Generate EfCore extensions for all entity types found
        // NOTE: This generator ONLY generates EfCore extensions (partial class extensions)
        // Base MetaType classes should be generated by the base generator
        if (entityTypeSet.Count > 0)
        {
            // Verify base types exist if RequireBaseTypes is enabled
            if (config.EfCore?.RequireBaseTypes == true)
            {
                var missingBaseTypes = VerifyBaseTypesExist(compilation, entityTypeSet);
                if (missingBaseTypes.Any())
                {
                    // Generate error diagnostic for missing base types
                    context.AddSource("_EfCoreMissingBaseTypes.g.cs", $@"
// ERROR: EfCore generator requires base MetaTypes but they were not found
// Missing base types for: {string.Join(", ", missingBaseTypes.Select(t => t.Name))}
//
// SOLUTIONS:
// 1. Configure base generator with BaseMetaTypes = true
// 2. Set EfCore.RequireBaseTypes = false to skip verification
// 3. Ensure base generator runs before EfCore generator
//
// Types discovered: {string.Join(", ", entityTypeSet.Select(t => t.Name))}
// Missing: {string.Join(", ", missingBaseTypes.Select(t => $"{t.Name}MetaType"))}
");
                    return; // Don't generate partial extensions without base classes
                }
            }
            
            foreach (var entityTypeSymbol in entityTypeSet)
            {
                // Generate extension using configured assembly name or entity's original assembly namespace
                var entityAssemblyName = !string.IsNullOrEmpty(config.AssemblyName) ? config.AssemblyName! : entityTypeSymbol.ContainingAssembly.Name;
                var efCoreExtensionSource = GenerateEfCoreExtension(entityTypeSymbol, entityAssemblyName);
                context.AddSource($"{entityTypeSymbol.Name}EfCoreMetaType.g.cs", efCoreExtensionSource);
            }
        }
    }


    private static string GenerateEfCoreExtension(INamedTypeSymbol typeSymbol, string assemblyNamespace)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using MetaTypes.Abstractions;");
        
        // For cross-assembly support, we need to ensure we have the right using statements
        var entityNamespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var entityAssemblyName = typeSymbol.ContainingAssembly.Name;
        
        // If the entity is from a different assembly, we need to reference its namespace
        if (entityAssemblyName != assemblyNamespace && !string.IsNullOrEmpty(entityNamespace))
        {
            // Add using for the entity's assembly namespace where MetaType classes are generated
            sb.AppendLine($"using {entityAssemblyName};");
        }
        
        sb.AppendLine();
        // Use the entity's assembly namespace where the original MetaType classes are generated
        // This allows partial classes to extend MetaTypes in their original namespace
        sb.AppendLine($"namespace {entityAssemblyName};");
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
        var keyProperties = properties.Where(IsKeyProperty).ToArray();
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
            
            // For now, we'll set ForeignKeyMember to null - this would need more complex analysis
            sb.AppendLine("    public IMetaTypeMember? ForeignKeyMember => null;");
            
            sb.AppendLine("}");
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    private static string? GetTableName(INamedTypeSymbol typeSymbol)
    {
        var tableAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.TableAttribute");
            
        if (tableAttribute?.ConstructorArguments.Length > 0)
        {
            var tableName = tableAttribute.ConstructorArguments[0].Value?.ToString();
            if (!string.IsNullOrEmpty(tableName))
            {
                return tableName;
            }
        }
        
        // Fallback to type name if no explicit table name
        return typeSymbol.Name;
    }

    private static bool IsKeyProperty(IPropertySymbol property)
    {
        // Check for [Key] attribute
        var keyAttribute = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.KeyAttribute");
            
        if (keyAttribute != null)
        {
            return true;
        }
        
        // EF Core convention: property named "Id" or "{TypeName}Id" is considered a key
        var typeName = property.ContainingType.Name;
        return string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(property.Name, $"{typeName}Id", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNotMappedProperty(IPropertySymbol property)
    {
        // Check for [NotMapped] attribute
        var notMappedAttribute = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute");
            
        return notMappedAttribute != null;
    }

    private static bool IsForeignKeyProperty(IPropertySymbol property)
    {
        // Simple heuristic: property name ends with "Id" and is not a primary key
        return property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && 
               !IsKeyProperty(property);
    }

    /// <summary>
    /// Verifies that base MetaType classes exist for the given entity types
    /// </summary>
    /// <param name="compilation">Current compilation</param>
    /// <param name="entityTypes">Entity types that need base MetaType classes</param>
    /// <returns>List of entity types that are missing their base MetaType classes</returns>
    private static List<INamedTypeSymbol> VerifyBaseTypesExist(Compilation compilation, HashSet<INamedTypeSymbol> entityTypes)
    {
        var missingBaseTypes = new List<INamedTypeSymbol>();
        
        foreach (var entityType in entityTypes)
        {
            // Construct the expected MetaType class name
            var metaTypeClassName = $"{entityType.Name}MetaType";
            
            // Look for the MetaType class in the entity's target namespace
            // Base generator puts MetaTypes in the target namespace (usually the assembly name)
            var targetNamespace = entityType.ContainingAssembly.Name;
            var expectedMetaTypeFullName = $"{targetNamespace}.{metaTypeClassName}";
            
            // Search for the MetaType class in the compilation
            var metaTypeSymbol = compilation.GetTypeByMetadataName(expectedMetaTypeFullName);
            
            if (metaTypeSymbol == null)
            {
                // Also try looking in the entity's original namespace as fallback
                var entityNamespace = entityType.ContainingNamespace.ToDisplayString();
                if (!string.IsNullOrEmpty(entityNamespace))
                {
                    var fallbackMetaTypeFullName = $"{entityNamespace}.{metaTypeClassName}";
                    metaTypeSymbol = compilation.GetTypeByMetadataName(fallbackMetaTypeFullName);
                }
            }
            
            if (metaTypeSymbol == null)
            {
                missingBaseTypes.Add(entityType);
            }
        }
        
        return missingBaseTypes;
    }
}