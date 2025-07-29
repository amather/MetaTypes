using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Discovery source indicates how a type was discovered.
/// </summary>
public enum DiscoverySource
{
    /// <summary>
    /// Type was discovered via syntax trees in the current compilation.
    /// </summary>
    Syntax,
    
    /// <summary>
    /// Type was discovered via metadata from referenced assemblies.
    /// </summary>
    Referenced
}

/// <summary>
/// Represents a discovered type with information about how and who discovered it.
/// </summary>
public class DiscoveredType
{
    /// <summary>
    /// The discovered type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; set; } = null!;
    
    /// <summary>
    /// How the type was discovered (syntax vs referenced assembly).
    /// </summary>
    public DiscoverySource Source { get; set; }
    
    /// <summary>
    /// Which component discovered this type (e.g., "Common", "EfCore").
    /// </summary>
    public string DiscoveredBy { get; set; } = null!;
    
    /// <summary>
    /// Additional context about the discovery method used.
    /// </summary>
    public string? DiscoveryContext { get; set; }
}

/// <summary>
/// Delegate for type discovery methods.
/// </summary>
/// <param name="compilation">The compilation context.</param>
/// <returns>Collection of discovered types.</returns>
public delegate IEnumerable<DiscoveredType> TypeDiscoverMethod(Compilation compilation);