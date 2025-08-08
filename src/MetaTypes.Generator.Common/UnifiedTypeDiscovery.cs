using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Unified type discovery system that coordinates multiple discovery methods.
/// Provides a consistent interface for both syntax-based and referenced-assembly discovery.
/// </summary>
public static class UnifiedTypeDiscovery
{
    /// <summary>
    /// Discovers types using configuration-driven discovery methods.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="config">Discovery configuration.</param>
    /// <returns>Unified list of discovered types with discovery metadata.</returns>
    public static IList<DiscoveredType> DiscoverTypes(Compilation compilation, IDiscoveryConfig config)
    {
        var discoveryMethods = GetConfiguredDiscoveryMethods(config);
        return DiscoverTypes(compilation, discoveryMethods.ToArray());
    }
    
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
    /// Gets discovery methods based on configuration.
    /// </summary>
    /// <param name="config">Discovery configuration.</param>
    /// <returns>List of configured discovery methods.</returns>
    public static List<TypeDiscoverMethod> GetConfiguredDiscoveryMethods(IDiscoveryConfig config)
    {
        var methods = new List<TypeDiscoverMethod>();
        
        // Add methods based on configuration flags
        if (config.Methods.MetaTypesAttributes && config.Syntax)
        {
            methods.Add(CommonDiscoveryMethods.DiscoverMetaTypesSyntax);
        }
        
        if (config.Methods.MetaTypesReferences && config.CrossAssembly)
        {
            methods.Add(CommonDiscoveryMethods.DiscoverMetaTypesReferenced);
        }
        
        // Handle EfCore methods if available in config
        if (config.Methods is EfCoreDiscoveryMethodsConfig efCoreConfig)
        {
            // Note: EfCore methods will be added by generators that have access to EfCore.Common
            // This is a hook for future extension by generators with EfCore capabilities
            var efCoreMethods = GetEfCoreDiscoveryMethods(efCoreConfig, config);
            methods.AddRange(efCoreMethods);
        }
        
        return methods;
    }
    
    /// <summary>
    /// Registry for EfCore discovery method providers. 
    /// Generators with EfCore capabilities can register their methods here.
    /// </summary>
    private static Func<EfCoreDiscoveryMethodsConfig, IDiscoveryConfig, IEnumerable<TypeDiscoverMethod>>? _efCoreMethodProvider;
    
    /// <summary>
    /// Registers an EfCore discovery method provider.
    /// Called by generators that have access to EfCore.Common.
    /// </summary>
    public static void RegisterEfCoreMethodProvider(
        Func<EfCoreDiscoveryMethodsConfig, IDiscoveryConfig, IEnumerable<TypeDiscoverMethod>> provider)
    {
        _efCoreMethodProvider = provider;
    }
    
    /// <summary>
    /// Gets EfCore discovery methods if a provider is registered.
    /// </summary>
    private static IEnumerable<TypeDiscoverMethod> GetEfCoreDiscoveryMethods(
        EfCoreDiscoveryMethodsConfig efCoreConfig, 
        IDiscoveryConfig config)
    {
        return _efCoreMethodProvider?.Invoke(efCoreConfig, config) ?? Enumerable.Empty<TypeDiscoverMethod>();
    }
    
    /// <summary>
    /// Gets the standard Common project discovery methods.
    /// Legacy method - prefer GetConfiguredDiscoveryMethods
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