using MetaTypes.Generator.Discovery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaTypes.Generator.Common.Vendor.Statics.Discovery;

/// <summary>
/// Discovery method that finds static classes containing static methods with StaticsServiceMethodAttribute.
/// Supports both syntax-based discovery (current compilation) and cross-assembly discovery (referenced assemblies).
/// </summary>
public class StaticsServiceMethodDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "Statics.ServiceMethod";

    public string Description => "Discovers static classes containing static methods with [StaticsServiceMethod] attribute";
    
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
    /// Discovers static classes from syntax trees in the current compilation.
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
                    if (IsStaticClassWithServiceMethods(typeDeclaration, typeSymbol, semanticModel))
                    {
                        discoveredTypes.Add(new DiscoveredType
                        {
                            TypeSymbol = typeSymbol,
                            Source = DiscoverySource.Syntax,
                            DiscoveredBy = [ Identifier ],
                            DiscoveryContexts = { [Identifier] = "Static class with StaticsServiceMethodAttribute via syntax" }
                        });
                    }
                }
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Discovers static classes from referenced assemblies.
    /// </summary>
    private IEnumerable<DiscoveredType> DiscoverFromReferencedAssemblies(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        var referencedTypes = GetAllTypesFromReferencedAssemblies(compilation);
        
        foreach (var typeSymbol in referencedTypes)
        {
            if (IsStaticClassWithServiceMethodsSymbol(typeSymbol))
            {
                discoveredTypes.Add(new DiscoveredType
                {
                    TypeSymbol = typeSymbol,
                    Source = DiscoverySource.Referenced,
                    DiscoveredBy = [ Identifier ],
                    DiscoveryContexts = { [Identifier] = "Static class with StaticsServiceMethodAttribute via assembly metadata" }
                });
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Checks if a type declaration is a static class containing static methods with StaticsServiceMethodAttribute.
    /// </summary>
    private static bool IsStaticClassWithServiceMethods(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, SemanticModel semanticModel)
    {
        // Check if class is static
        if (!typeSymbol.IsStatic)
            return false;
        
        // Check if class is public or internal
        if (typeSymbol.DeclaredAccessibility != Accessibility.Public && 
            typeSymbol.DeclaredAccessibility != Accessibility.Internal)
            return false;
        
        // Check if any static methods have StaticsServiceMethodAttribute
        foreach (var methodDeclaration in typeDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (semanticModel.GetDeclaredSymbol(methodDeclaration) is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.IsStatic && HasStaticsServiceMethodAttribute(methodDeclaration, semanticModel))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a type symbol is a static class containing static methods with StaticsServiceMethodAttribute.
    /// </summary>
    private static bool IsStaticClassWithServiceMethodsSymbol(INamedTypeSymbol typeSymbol)
    {
        // Check if class is static
        if (!typeSymbol.IsStatic)
            return false;
        
        // Check if class is public or internal
        if (typeSymbol.DeclaredAccessibility != Accessibility.Public && 
            typeSymbol.DeclaredAccessibility != Accessibility.Internal)
            return false;
        
        // Check if any static methods have StaticsServiceMethodAttribute
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IMethodSymbol methodSymbol && methodSymbol.IsStatic)
            {
                if (HasStaticsServiceMethodAttributeSymbol(methodSymbol))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a method declaration has the StaticsServiceMethodAttribute via syntax analysis.
    /// </summary>
    private static bool HasStaticsServiceMethodAttribute(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        foreach (var attributeList in methodDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol attributeConstructor)
                {
                    var attributeContainingTypeSymbol = attributeConstructor.ContainingType;
                    var fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute")
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a method symbol has the StaticsServiceMethodAttribute via symbol metadata.
    /// </summary>
    private static bool HasStaticsServiceMethodAttributeSymbol(IMethodSymbol methodSymbol)
    {
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute")
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