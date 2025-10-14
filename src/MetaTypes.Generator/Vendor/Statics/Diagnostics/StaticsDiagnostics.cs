using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Vendor.Statics.Diagnostics;

/// <summary>
/// Diagnostic descriptors for Statics vendor functionality.
/// All diagnostic IDs start with MTSTAT.
/// </summary>
public static class StaticsDiagnostics
{
    private const string Category = "MetaTypes.Statics";

    // Repository diagnostics (MTSTAT0001-0099)

    public static readonly DiagnosticDescriptor MTSTAT0001_RepositoryProviderOnNonDbContext = new(
        id: "MTSTAT0001",
        title: "StaticsRepositoryProvider on non-DbContext type",
        messageFormat: "Type '{0}' has [StaticsRepositoryProvider] attribute but does not inherit from DbContext",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The StaticsRepositoryProvider attribute should only be used on DbContext types.");

    public static readonly DiagnosticDescriptor MTSTAT0002_RepositoryIgnoreOnNonDbSet = new(
        id: "MTSTAT0002",
        title: "StaticsRepositoryIgnore on non-DbSet property",
        messageFormat: "Property '{0}' has [StaticsRepositoryIgnore] attribute but is not a DbSet<T> property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The StaticsRepositoryIgnore attribute is intended for DbSet<T> properties.");
}
