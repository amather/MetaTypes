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
/// Supports aggregation when a type is found by multiple discovery methods.
/// </summary>
public class DiscoveredType
{
    /// <summary>
    /// The discovered type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; set; } = null!;
    
    /// <summary>
    /// How the type was discovered (syntax vs referenced assembly).
    /// When aggregated, this represents the primary source, with preference for syntax discovery.
    /// </summary>
    public DiscoverySource Source { get; set; }
    
    /// <summary>
    /// All discovery methods that found this type (e.g., ["MetaTypes.Attribute", "EfCore.TableAttribute"]).
    /// </summary>
    public string[] DiscoveredBy { get; set; } = null!;
    
    /// <summary>
    /// Additional context about the discovery methods used, keyed by discovery method identifier.
    /// </summary>
    public Dictionary<string, string?> DiscoveryContexts { get; set; } = new();
    
    /// <summary>
    /// Convenience property that returns the primary discovery method (first one).
    /// </summary>
    public string PrimaryDiscoveredBy => DiscoveredBy?.FirstOrDefault() ?? "Unknown";
    
    /// <summary>
    /// Checks if this type was discovered by a specific method.
    /// </summary>
    /// <param name="methodIdentifier">The discovery method identifier to check.</param>
    /// <returns>True if the type was discovered by the specified method.</returns>
    public bool WasDiscoveredBy(string methodIdentifier)
    {
        return DiscoveredBy?.Contains(methodIdentifier, StringComparer.OrdinalIgnoreCase) ?? false;
    }
    
    /// <summary>
    /// Checks if this type was discovered by any method matching the given prefix.
    /// </summary>
    /// <param name="methodPrefix">The method prefix to match (e.g., "EfCore.").</param>
    /// <returns>True if any discovery method starts with the prefix.</returns>
    public bool WasDiscoveredByPrefix(string methodPrefix)
    {
        return DiscoveredBy?.Any(method => method.StartsWith(methodPrefix, StringComparison.OrdinalIgnoreCase)) ?? false;
    }
}

