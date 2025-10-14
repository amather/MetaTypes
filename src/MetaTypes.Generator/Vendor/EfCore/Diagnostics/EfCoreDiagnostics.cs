using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Vendor.EfCore.Diagnostics;

/// <summary>
/// Diagnostic descriptors for EfCore vendor functionality.
/// All diagnostic IDs start with MTEFCORE.
/// </summary>
public static class EfCoreDiagnostics
{
    private const string Category = "MetaTypes.EfCore";

    // DbContext diagnostics (MTEFCORE0001-0099)

    public static readonly DiagnosticDescriptor MTEFCORE0001_DbContextNotInherited = new(
        id: "MTEFCORE0001",
        title: "Type must inherit from DbContext",
        messageFormat: "Type '{0}' is discovered as EfCore type but does not inherit from DbContext",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types discovered via EfCore discovery methods must inherit from DbContext.");

    public static readonly DiagnosticDescriptor MTEFCORE0002_EmptyDbContext = new(
        id: "MTEFCORE0002",
        title: "DbContext has no DbSet properties",
        messageFormat: "DbContext '{0}' has no DbSet<T> properties",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A DbContext without DbSet properties will not generate useful metadata.");

    public static readonly DiagnosticDescriptor MTEFCORE0003_MissingMetaTypeOnEntity = new(
        id: "MTEFCORE0003",
        title: "Entity type missing MetaType attribute",
        messageFormat: "Entity type '{0}' in DbSet<{0}> does not have [MetaType] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Consider adding [MetaType] attribute to entity types for complete metadata generation.");

    // Entity diagnostics (MTEFCORE0100-0199)

    public static readonly DiagnosticDescriptor MTEFCORE0100_MissingKeyAttribute = new(
        id: "MTEFCORE0100",
        title: "Entity missing key property",
        messageFormat: "Entity '{0}' has no property marked with [Key] attribute or conventional 'Id' property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Entity types should have a key property for EF Core to function correctly.");

    public static readonly DiagnosticDescriptor MTEFCORE0101_MultipleKeyAttributes = new(
        id: "MTEFCORE0101",
        title: "Entity has multiple key properties",
        messageFormat: "Entity '{0}' has multiple properties marked with [Key] attribute. Consider using composite keys or single key.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Multiple key attributes detected. Ensure this is intentional for composite keys.");
}
