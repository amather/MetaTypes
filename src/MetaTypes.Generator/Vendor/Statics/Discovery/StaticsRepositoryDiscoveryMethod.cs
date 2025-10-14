using MetaTypes.Generator.Discovery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaTypes.Generator.Common.Vendor.Statics.Discovery;

/// <summary>
/// Discovery method that finds classes with StaticsRepositoryProviderAttribute.
/// Supports both syntax-based discovery (current compilation) and cross-assembly discovery (referenced assemblies).
/// Records discovered types with their specific role (e.g., RepositoryProvider) for future extensibility.
/// </summary>
public class StaticsRepositoryDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "Statics.Repository";

    public string Description => "Discovers classes with [StaticsRepositoryProvider] attribute";

    public bool RequiresCrossAssembly => false; // Supports both modes

    public IEnumerable<DiscoveredType> Discover(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();

        // Syntax-based discovery (current compilation)
        discoveredTypes.AddRange(DiscoverFromSyntax(compilation));

        // Cross-assembly discovery (referenced assemblies)
        discoveredTypes.AddRange(DiscoverFromReferencedAssemblies(compilation));

        return discoveredTypes;
    }

    /// <summary>
    /// Discovers classes from syntax trees in the current compilation.
    /// </summary>
    private IEnumerable<DiscoveredType> DiscoverFromSyntax(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var typeDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol)
                {
                    if (HasStaticsRepositoryProviderAttribute(typeDeclaration, semanticModel))
                    {
                        discoveredTypes.Add(new DiscoveredType
                        {
                            TypeSymbol = typeSymbol,
                            Source = DiscoverySource.Syntax,
                            DiscoveredBy = [ Identifier ],
                            DiscoveryContexts = { [Identifier] = "RepositoryProvider" }
                        });
                    }
                }
            }
        }

        return discoveredTypes;
    }

    /// <summary>
    /// Discovers classes from referenced assemblies.
    /// </summary>
    private IEnumerable<DiscoveredType> DiscoverFromReferencedAssemblies(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();

        var referencedTypes = GetAllTypesFromReferencedAssemblies(compilation);

        foreach (var typeSymbol in referencedTypes)
        {
            if (HasStaticsRepositoryProviderAttributeSymbol(typeSymbol))
            {
                discoveredTypes.Add(new DiscoveredType
                {
                    TypeSymbol = typeSymbol,
                    Source = DiscoverySource.Referenced,
                    DiscoveredBy = [ Identifier ],
                    DiscoveryContexts = { [Identifier] = "RepositoryProvider" }
                });
            }
        }

        return discoveredTypes;
    }

    /// <summary>
    /// Checks if a type declaration has the StaticsRepositoryProviderAttribute via syntax analysis.
    /// </summary>
    private static bool HasStaticsRepositoryProviderAttribute(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
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

                    if (fullName == "Statics.ServiceBroker.Attributes.StaticsRepositoryProviderAttribute")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a type symbol has the StaticsRepositoryProviderAttribute via symbol metadata.
    /// </summary>
    private static bool HasStaticsRepositoryProviderAttributeSymbol(INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsRepositoryProviderAttribute")
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
