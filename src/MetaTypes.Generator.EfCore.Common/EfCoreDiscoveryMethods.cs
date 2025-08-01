using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MetaTypes.Generator.Common;

namespace MetaTypes.Generator.EfCore.Common;

/// <summary>
/// EfCore-specific discovery methods for finding entity types.
/// This is shared between the base generator (when EfCore detection is enabled) 
/// and the EfCore extension generator.
/// </summary>
public static class EfCoreDiscoveryMethods
{
    /// <summary>
    /// Discovers entity types via [Table] attribute in syntax trees.
    /// </summary>
    public static IEnumerable<DiscoveredType> DiscoverTableEntityTypesSyntax(Compilation compilation)
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
                            DiscoveredBy = "EfCore",
                            DiscoveryContext = "Table attribute via syntax"
                        });
                    }
                }
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Discovers entity types via DbContext analysis in syntax trees.
    /// This method can discover entities from referenced assemblies via DbSet properties.
    /// </summary>
    public static IEnumerable<DiscoveredType> DiscoverDbContextEntityTypes(Compilation compilation)
    {
        var discoveredTypes = new List<DiscoveredType>();
        
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
                {
                    if (IsDbContext(classSymbol))
                    {
                        var entityTypes = ExtractDbSetEntityTypes(classSymbol);
                        foreach (var entityType in entityTypes)
                        {
                            var source = entityType.ContainingAssembly.Equals(compilation.Assembly, SymbolEqualityComparer.Default)
                                ? DiscoverySource.Syntax
                                : DiscoverySource.Referenced;
                                
                            discoveredTypes.Add(new DiscoveredType
                            {
                                TypeSymbol = entityType,
                                Source = source,
                                DiscoveredBy = "EfCore",
                                DiscoveryContext = $"DbSet property in {classSymbol.Name}"
                            });
                        }
                    }
                }
            }
        }
        
        return discoveredTypes;
    }
    
    /// <summary>
    /// Gets the standard EfCore discovery methods.
    /// </summary>
    public static TypeDiscoverMethod[] GetEfCoreDiscoveryMethods()
    {
        return new TypeDiscoverMethod[]
        {
            DiscoverTableEntityTypesSyntax,
            DiscoverDbContextEntityTypes
        };
    }
    
    /// <summary>
    /// Checks if a type declaration has the [Table] attribute.
    /// </summary>
    private static bool HasTableAttribute(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
    {
        foreach (var attributeList in typeDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (semanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeSymbol)
                {
                    var fullName = attributeSymbol.ContainingType.ToDisplayString();
                    if (fullName == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a type is a DbContext.
    /// </summary>
    private static bool IsDbContext(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (current.Name == "DbContext" && 
                current.ContainingNamespace.ToDisplayString() == "Microsoft.EntityFrameworkCore")
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
    
    /// <summary>
    /// Extracts entity types from DbSet properties in a DbContext.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> ExtractDbSetEntityTypes(INamedTypeSymbol dbContextType)
    {
        var entityTypes = new List<INamedTypeSymbol>();
        
        foreach (var member in dbContextType.GetMembers())
        {
            if (member is IPropertySymbol property && 
                property.Type is INamedTypeSymbol propertyType &&
                IsDbSetType(propertyType))
            {
                // Extract T from DbSet<T>
                if (propertyType.TypeArguments.Length > 0 &&
                    propertyType.TypeArguments[0] is INamedTypeSymbol entityType)
                {
                    entityTypes.Add(entityType);
                }
            }
        }
        
        return entityTypes;
    }
    
    /// <summary>
    /// Checks if a type is DbSet<T>.
    /// </summary>
    private static bool IsDbSetType(INamedTypeSymbol type)
    {
        return type.Name == "DbSet" && 
               type.ContainingNamespace.ToDisplayString() == "Microsoft.EntityFrameworkCore";
    }
}