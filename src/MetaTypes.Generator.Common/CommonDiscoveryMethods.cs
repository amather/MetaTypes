using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Standard discovery methods provided by the Common project.
/// </summary>
public static class CommonDiscoveryMethods
{
    /// <summary>
    /// Discovers types with [MetaType] attribute via syntax trees in current compilation.
    /// </summary>
    public static IEnumerable<DiscoveredType> DiscoverMetaTypesSyntax(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            foreach (var typeDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol)
                {
                    if (HasMetaTypeAttributeSyntax(typeDeclaration, semanticModel))
                    {
                        discoveredTypes.Add(new DiscoveredType
                        {
                            TypeSymbol = typeSymbol,
                            Source = DiscoverySource.Syntax,
                            DiscoveredBy = "Common",
                            DiscoveryContext = "MetaType attribute via syntax"
                        });
                    }
                }
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Discovers types with [MetaType] attribute from referenced assemblies.
    /// </summary>
    public static IEnumerable<DiscoveredType> DiscoverMetaTypesReferenced(Compilation compilation)
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
                    DiscoveredBy = "Common",
                    DiscoveryContext = "MetaType attribute via assembly metadata"
                });
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Checks if a type declaration has the [MetaType] attribute via syntax analysis.
    /// </summary>
    private static bool HasMetaTypeAttributeSyntax(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
    {
        foreach (var attributeList in typeDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol attributeConstructor)
                {
                    var attributeContainingTypeSymbol = attributeConstructor.ContainingType;
                    var fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName == "MetaTypes.Abstractions.MetaTypeAttribute")
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
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