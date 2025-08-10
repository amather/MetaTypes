using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MetaTypes.Generator.Common;

namespace MetaTypes.Generator.Common.Vendor.EfCore.Discovery;

/// <summary>
/// Discovery method that finds EF Core entity types by scanning DbContext properties.
/// Analyzes DbContext classes to discover entity types via DbSet&lt;T&gt; properties.
/// </summary>
public class DbContextScanningDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "EfCore.DbContextSet";
    
    public string Description => "Discovers EF Core entity types by scanning DbContext DbSet properties";
    
    public bool RequiresCrossAssembly => true; // May need to look at referenced DbContexts
    
    public bool CanRun(Compilation compilation) => true;
    
    public IEnumerable<DiscoveredType> Discover(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        // Find all DbContext types first
        var dbContextTypes = FindDbContextTypes(compilation);
        
        foreach (var dbContextType in dbContextTypes)
        {
            var entityTypes = GetEntityTypesFromDbContext(dbContextType);
            discoveredTypes.AddRange(entityTypes);
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Finds all DbContext types in the compilation.
    /// </summary>
    private IEnumerable<INamedTypeSymbol> FindDbContextTypes(Compilation compilation)
    {
        var dbContextTypes = new List<INamedTypeSymbol>();
        
        // Check syntax trees for DbContext classes
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
                {
                    if (IsDbContextType(classSymbol))
                    {
                        dbContextTypes.Add(classSymbol);
                    }
                }
            }
        }
        
        // Also check referenced assemblies for DbContext types
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                if (IsSystemAssembly(assembly.Name))
                    continue;
                    
                var referencedDbContexts = FindDbContextTypesInNamespace(assembly.GlobalNamespace);
                dbContextTypes.AddRange(referencedDbContexts);
            }
        }
        
        return dbContextTypes;
    }
    
    /// <summary>
    /// Recursively finds DbContext types in a namespace.
    /// </summary>
    private IEnumerable<INamedTypeSymbol> FindDbContextTypesInNamespace(INamespaceSymbol namespaceSymbol)
    {
        var dbContexts = new List<INamedTypeSymbol>();
        
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (IsDbContextType(type))
            {
                dbContexts.Add(type);
            }
        }
        
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            dbContexts.AddRange(FindDbContextTypesInNamespace(nestedNamespace));
        }
        
        return dbContexts;
    }
    
    /// <summary>
    /// Checks if a type is a DbContext.
    /// </summary>
    private static bool IsDbContextType(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == "Microsoft.EntityFrameworkCore.DbContext")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
    
    /// <summary>
    /// Gets entity types from a DbContext by analyzing its DbSet properties.
    /// </summary>
    private IEnumerable<DiscoveredType> GetEntityTypesFromDbContext(INamedTypeSymbol dbContextType)
    {
        var entityTypes = new List<DiscoveredType>();
        
        foreach (var member in dbContextType.GetMembers().OfType<IPropertySymbol>())
        {
            if (IsDbSetProperty(member))
            {
                var entityType = GetEntityTypeFromDbSet(member);
                if (entityType != null)
                {
                    entityTypes.Add(new DiscoveredType
                    {
                        TypeSymbol = entityType,
                        Source = DiscoverySource.Referenced, // DbContext scanning is considered referenced
                        DiscoveredBy = new[] { Identifier },
                        DiscoveryContexts = { [Identifier] = $"DbSet property in {dbContextType.Name}" }
                    });
                }
            }
        }
        
        return entityTypes;
    }
    
    /// <summary>
    /// Checks if a property is a DbSet&lt;T&gt; property.
    /// </summary>
    private static bool IsDbSetProperty(IPropertySymbol property)
    {
        if (property.Type is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.ToDisplayString() == "Microsoft.EntityFrameworkCore.DbSet<T>")
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Extracts the entity type T from a DbSet&lt;T&gt; property.
    /// </summary>
    private static INamedTypeSymbol? GetEntityTypeFromDbSet(IPropertySymbol dbSetProperty)
    {
        if (dbSetProperty.Type is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0] as INamedTypeSymbol;
        }
        
        return null;
    }
    
    /// <summary>
    /// Checks if an assembly is a system assembly to skip during discovery.
    /// </summary>
    private static bool IsSystemAssembly(string assemblyName)
    {
        return assemblyName.StartsWith("System.") || 
               assemblyName.StartsWith("Microsoft.") || 
               assemblyName == "mscorlib" || 
               assemblyName == "netstandard";
    }
}