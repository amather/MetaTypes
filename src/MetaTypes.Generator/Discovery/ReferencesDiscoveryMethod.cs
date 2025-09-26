using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Discovery method that finds types with [MetaType] attribute from referenced assemblies.
/// Scans assembly metadata to find pre-compiled types with MetaType attributes.
/// </summary>
public class ReferencesDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "MetaTypes.Reference";
    
    public string Description => "Discovers types with [MetaType] attribute from referenced assemblies";
    
    public bool RequiresCrossAssembly => true;
    
    public bool CanRun(Compilation compilation) => true;
    
    public IEnumerable<DiscoveredType> Discover(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        // Get all types from referenced assemblies
        var referencedTypes = GetAllTypesFromReferencedAssemblies(compilation);
        
        foreach (var typeSymbol in referencedTypes)
        {
            if (HasMetaTypeAttributeSymbol(typeSymbol))
            {
                discoveredTypes.Add(new DiscoveredType
                {
                    TypeSymbol = typeSymbol,
                    Source = DiscoverySource.Referenced,
                    DiscoveredBy = new[] { Identifier },
                    DiscoveryContexts = { [Identifier] = "MetaType attribute via assembly metadata" }
                });
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Checks if a type symbol has the [MetaType] attribute via symbol metadata.
    /// </summary>
    private static bool HasMetaTypeAttributeSymbol(INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == "MetaTypes.Abstractions.MetaTypeAttribute")
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Gets all types from referenced assemblies.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAllTypesFromReferencedAssemblies(Compilation compilation)
    {
        var types = new List<INamedTypeSymbol>();
        
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                // Skip system assemblies to improve performance
                if (IsSystemAssembly(assembly.Name))
                    continue;
                    
                AddTypesFromNamespace(assembly.GlobalNamespace, types);
            }
        }
        
        return types;
    }
    
    /// <summary>
    /// Recursively adds types from a namespace.
    /// </summary>
    private static void AddTypesFromNamespace(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> types)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            types.Add(type);
        }
        
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            AddTypesFromNamespace(nestedNamespace, types);
        }
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