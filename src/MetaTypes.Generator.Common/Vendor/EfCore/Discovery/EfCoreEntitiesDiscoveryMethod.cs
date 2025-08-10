using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MetaTypes.Generator.Common;

namespace MetaTypes.Generator.Common.Vendor.EfCore.Discovery;

/// <summary>
/// Discovery method that finds EF Core entity types via [Table] attribute syntax analysis.
/// Scans syntax trees for types decorated with Entity Framework [Table] attributes.
/// </summary>
public class EfCoreEntitiesDiscoveryMethod : IDiscoveryMethod
{
    public string Identifier => "EfCore.TableAttribute";
    
    public string Description => "Discovers EF Core entity types with [Table] attribute via syntax analysis";
    
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
                    if (HasTableAttribute(typeDeclaration, semanticModel))
                    {
                        discoveredTypes.Add(new DiscoveredType
                        {
                            TypeSymbol = typeSymbol,
                            Source = DiscoverySource.Syntax,
                            DiscoveredBy = new[] { Identifier },
                            DiscoveryContexts = { [Identifier] = "EF Core [Table] attribute via syntax" }
                        });
                    }
                }
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Checks if a type declaration has the [Table] attribute via syntax analysis.
    /// </summary>
    private static bool HasTableAttribute(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
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

                    if (fullName == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
}