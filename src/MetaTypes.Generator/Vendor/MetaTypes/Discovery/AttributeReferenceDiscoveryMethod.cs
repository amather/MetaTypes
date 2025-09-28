using MetaTypes.Generator.Discovery;
using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Vendor.MetaTypes.Discovery;

/// <summary>
/// Discovery method that finds types with [MetaType] attribute from referenced assemblies.
/// Scans assembly metadata to find pre-compiled types with MetaType attributes.
/// </summary>
public class AttributeReferenceDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "MetaTypes.Attribute.Reference";
    
    public string Description => "Discovers types with [MetaType] attribute from referenced assemblies";
    
    public bool RequiresCrossAssembly => true;
    
    public IEnumerable<DiscoveredType> Discover(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        // get all types from all referenced assemblies
        var referencedTypes = GetAllTypesFromReferencedAssemblies(compilation);

        foreach (var typeSymbol in referencedTypes)
        {
            if (HasMetaTypeAttributeSymbol(typeSymbol) == false)
            {
                continue;
            }

            discoveredTypes.Add(new DiscoveredType
            {
                TypeSymbol = typeSymbol,
                Source = DiscoverySource.Referenced,
                DiscoveredBy = [Identifier],
                DiscoveryContexts = { [Identifier] = "MetaType attribute via assembly metadata" }
            });
        }
        
        return discoveredTypes;
    }
    
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
    
    private static bool IsSystemAssembly(string assemblyName)
    {
        return assemblyName.StartsWith("System.") || 
               assemblyName.StartsWith("Microsoft.") || 
               assemblyName == "mscorlib" || 
               assemblyName == "netstandard";
    }
}