using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Vendor.Statics.Diagnostics;

/// <summary>
/// Diagnostic descriptors for Statics vendor functionality.
/// All diagnostic IDs start with MTSTAT.
/// </summary>
public static class StaticsDiagnostics
{
    private const string Category = "MetaTypes.Statics";

    // ServiceMethod diagnostics (MTSTAT0001-0099)

    public static readonly DiagnosticDescriptor MTSTAT0001_InvalidReturnType = new(
        id: "MTSTAT0001",
        title: "Invalid return type for StaticsServiceMethod",
        messageFormat: "Method '{0}' must return ServiceResult<T> or ServiceResult (async via Task<T> is supported), but returns '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Methods with [StaticsServiceMethod] attribute must return ServiceResult<T> or ServiceResult.");

    public static readonly DiagnosticDescriptor MTSTAT0002_MissingRouteParameter = new(
        id: "MTSTAT0002",
        title: "Route parameter missing from method signature",
        messageFormat: "Route parameter '{0}' from path '{1}' must exist as a method parameter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All route parameters in the Path must have corresponding method parameters.");

    public static readonly DiagnosticDescriptor MTSTAT0003_MissingEntityParameter = new(
        id: "MTSTAT0003",
        title: "Methods with 'id' parameter must specify Entity",
        messageFormat: "Method '{0}' has 'id' parameter but does not specify Entity parameter in [StaticsServiceMethod] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Methods with an 'id' parameter must specify which entity they operate on via the Entity parameter.");

    public static readonly DiagnosticDescriptor MTSTAT0004_EntityGlobalWithIdParameter = new(
        id: "MTSTAT0004",
        title: "EntityGlobal with 'id' parameter is invalid",
        messageFormat: "Method '{0}' has EntityGlobal = true but also has 'id' parameter. EntityGlobal methods should not have entity-specific parameters.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Methods marked with EntityGlobal = true should not have 'id' parameters as they are not entity-specific.");

    public static readonly DiagnosticDescriptor MTSTAT0005_EntityWithIdShouldNotBeGlobal = new(
        id: "MTSTAT0005",
        title: "Entity with 'id' parameter should not be EntityGlobal",
        messageFormat: "Method '{0}' has Entity parameter and 'id' parameter but EntityGlobal = true. This configuration is contradictory.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Entity-specific methods (with 'id' parameter) should not be marked as EntityGlobal.");

    public static readonly DiagnosticDescriptor MTSTAT0006_RouteParameterTypeMismatch = new(
        id: "MTSTAT0006",
        title: "Route parameter type constraint does not match method parameter type",
        messageFormat: "Route parameter '{0}:{1}' type constraint does not match method parameter type '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Route parameter type constraints (e.g., {id:int}) must match the actual method parameter types.");

    public static readonly DiagnosticDescriptor MTSTAT0007_MissingPathParameter = new(
        id: "MTSTAT0007",
        title: "Path parameter is required",
        messageFormat: "Method '{0}' with [StaticsServiceMethod] attribute must specify a Path parameter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Path parameter is required for all StaticsServiceMethod attributes.");

    // Repository diagnostics (MTSTAT0100-0199)

    public static readonly DiagnosticDescriptor MTSTAT0100_RepositoryProviderOnNonDbContext = new(
        id: "MTSTAT0100",
        title: "StaticsRepositoryProvider on non-DbContext type",
        messageFormat: "Type '{0}' has [StaticsRepositoryProvider] attribute but does not inherit from DbContext",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The StaticsRepositoryProvider attribute should only be used on DbContext types.");

    public static readonly DiagnosticDescriptor MTSTAT0101_RepositoryIgnoreOnNonDbSet = new(
        id: "MTSTAT0101",
        title: "StaticsRepositoryIgnore on non-DbSet property",
        messageFormat: "Property '{0}' has [StaticsRepositoryIgnore] attribute but is not a DbSet<T> property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The StaticsRepositoryIgnore attribute is intended for DbSet<T> properties.");
}
