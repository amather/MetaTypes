using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Discovery method that finds types with [MetaType] attribute via syntax analysis.
/// Scans syntax trees in the current compilation for attribute decorations.
/// </summary>
public class AttributesDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "MetaTypes.Attribute";
    
    public string Description => "Discovers types with [MetaType] attribute via syntax analysis";
    
    public bool RequiresCrossAssembly => false;
    
    public bool CanRun(Compilation compilation) => true;
    
    public IEnumerable<DiscoveredType> Discover(Compilation compilation)
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
                            DiscoveredBy = new[] { Identifier },
                            DiscoveryContexts = { [Identifier] = "MetaType attribute via syntax" }
                        });
                    }
                }
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
}