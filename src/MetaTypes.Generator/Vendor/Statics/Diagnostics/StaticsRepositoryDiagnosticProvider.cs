using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MetaTypes.Generator.Diagnostics;

namespace MetaTypes.Generator.Vendor.Statics.Diagnostics;

/// <summary>
/// Diagnostic analyzer provider for Statics.Repository discovery method.
/// Validates [StaticsRepositoryProvider] and [StaticsRepositoryIgnore] attribute usage.
/// </summary>
public class StaticsRepositoryDiagnosticProvider : IDiagnosticAnalyzerProvider
{
    public string Identifier => "Statics.Repository";

    public string Description => "Validates StaticsRepositoryProvider and StaticsRepositoryIgnore attribute usage";

    public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => new[]
    {
        StaticsDiagnostics.MTSTAT0001_RepositoryProviderOnNonDbContext,
        StaticsDiagnostics.MTSTAT0002_RepositoryIgnoreOnNonDbSet,
    };

    public void Analyze(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        ImmutableArray<AttributeData> relevantAttributes)
    {
        // Check if type has StaticsRepositoryProviderAttribute
        var hasRepositoryProviderAttribute = typeSymbol.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsRepositoryProviderAttribute");

        if (hasRepositoryProviderAttribute)
        {
            ValidateRepositoryProvider(context, typeSymbol);
        }

        // Check properties for StaticsRepositoryIgnoreAttribute
        ValidateRepositoryIgnoreAttributes(context, typeSymbol);
    }

    /// <summary>
    /// Validates that StaticsRepositoryProvider attribute is only used on DbContext types.
    /// </summary>
    private void ValidateRepositoryProvider(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        // Check if type inherits from DbContext
        if (!InheritsFromDbContext(typeSymbol))
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0001_RepositoryProviderOnNonDbContext,
                typeSymbol.Locations[0],
                typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Validates that StaticsRepositoryIgnore attribute is only used on DbSet properties.
    /// </summary>
    private void ValidateRepositoryIgnoreAttributes(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        // Check all properties for StaticsRepositoryIgnoreAttribute
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IPropertySymbol propertySymbol)
            {
                var hasIgnoreAttribute = propertySymbol.GetAttributes().Any(a =>
                    a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsRepositoryIgnoreAttribute");

                if (hasIgnoreAttribute)
                {
                    // Check if the property is a DbSet<T>
                    if (!IsDbSetProperty(propertySymbol))
                    {
                        var diagnostic = Diagnostic.Create(
                            StaticsDiagnostics.MTSTAT0002_RepositoryIgnoreOnNonDbSet,
                            propertySymbol.Locations[0],
                            propertySymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if a type inherits from Microsoft.EntityFrameworkCore.DbContext.
    /// </summary>
    private bool InheritsFromDbContext(INamedTypeSymbol typeSymbol)
    {
        var currentType = typeSymbol.BaseType;

        while (currentType != null)
        {
            var fullName = currentType.ToDisplayString();

            if (fullName == "Microsoft.EntityFrameworkCore.DbContext")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks if a property is a DbSet<T> property.
    /// </summary>
    private bool IsDbSetProperty(IPropertySymbol propertySymbol)
    {
        var propertyType = propertySymbol.Type;

        if (propertyType is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            // Get the unbound generic type (e.g., DbSet<> from DbSet<User>)
            var unboundType = namedType.ConstructUnboundGenericType();
            var fullName = unboundType.ToDisplayString();

            // Check if it's DbSet<>
            if (fullName == "Microsoft.EntityFrameworkCore.DbSet<>")
            {
                return true;
            }
        }

        return false;
    }
}
