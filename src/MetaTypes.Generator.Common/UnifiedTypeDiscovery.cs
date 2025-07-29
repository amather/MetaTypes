using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Unified type discovery system that coordinates multiple discovery methods.
/// Provides a consistent interface for both syntax-based and referenced-assembly discovery.
/// </summary>
public static class UnifiedTypeDiscovery
{
    /// <summary>
    /// Discovers types using multiple discovery methods and returns a unified list.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="discoveryMethods">The discovery methods to use.</param>
    /// <returns>Unified list of discovered types with discovery metadata.</returns>
    public static IList<DiscoveredType> DiscoverTypes(Compilation compilation, params TypeDiscoverMethod[] discoveryMethods)
    {
        var allDiscoveredTypes = new List<DiscoveredType>();
        
        foreach (var discoveryMethod in discoveryMethods)
        {
            try
            {
                var discoveredTypes = discoveryMethod(compilation);
                allDiscoveredTypes.AddRange(discoveredTypes);
            }
            catch (Exception)
            {
                // Silently continue with other discovery methods
                // Source generators should not produce console output
            }
        }
        
        // Remove duplicates based on type symbol equality, preferring syntax discovery over referenced
        var deduplicated = allDiscoveredTypes
            .GroupBy(dt => dt.TypeSymbol, SymbolEqualityComparer.Default)
            .Select(group => group
                .OrderBy(dt => dt.Source == DiscoverySource.Syntax ? 0 : 1) // Prefer syntax discovery
                .First())
            .ToList();
            
        return deduplicated;
    }
    
    /// <summary>
    /// Gets the standard Common project discovery methods.
    /// </summary>
    /// <returns>Array of Common discovery methods.</returns>
    public static TypeDiscoverMethod[] GetCommonDiscoveryMethods()
    {
        return new TypeDiscoverMethod[]
        {
            CommonDiscoveryMethods.DiscoverMetaTypesSyntax,
            CommonDiscoveryMethods.DiscoverMetaTypesReferenced
        };
    }
}