using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Unified type discovery system that auto-discovers and coordinates multiple discovery methods via reflection.
/// Provides a consistent interface for both syntax-based and referenced-assembly discovery.
/// </summary>
public static class UnifiedTypeDiscovery
{
    private static readonly ConcurrentDictionary<string, IDiscoveryMethod> _discoveredMethods 
        = new(StringComparer.OrdinalIgnoreCase);
    
    private static readonly object _initializationLock = new();
    private static bool _initialized = false;
    
    /// <summary>
    /// Discovers types using configuration-driven discovery methods via auto-discovered plugins.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="config">Discovery configuration.</param>
    /// <returns>Unified list of discovered types with discovery metadata.</returns>
    public static IList<DiscoveredType> DiscoverTypes(Compilation compilation, IDiscoveryConfig config)
    {
        var result = GetDiscoveryResult(compilation, config);
        return result.DiscoveredTypes;
    }
    
    /// <summary>
    /// Gets detailed discovery results including warnings and errors from the auto-discovered plugin system.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="config">Discovery configuration.</param>
    /// <returns>Detailed discovery execution result with diagnostics.</returns>
    public static DiscoveryExecutionResult GetDiscoveryResult(Compilation compilation, IDiscoveryConfig config)
    {
        EnsureDiscoveryMethodsLoaded();
        
        // Filter methods based on cross-assembly requirements
        var configuredMethods = config.Methods.Methods;
        
        if (!config.CrossAssembly)
        {
            // Only include methods that don't require cross-assembly discovery
            configuredMethods = configuredMethods
                .Where(methodId => !_discoveredMethods.TryGetValue(methodId, out var method) || !method.RequiresCrossAssembly)
                .ToArray();
        }
        
        return ExecuteDiscoveryMethods(configuredMethods, compilation);
    }
    
    /// <summary>
    /// Ensures discovery methods are loaded via reflection (called once per application domain).
    /// </summary>
    private static void EnsureDiscoveryMethodsLoaded()
    {
        if (_initialized) return;
        
        lock (_initializationLock)
        {
            if (_initialized) return;
            
            LoadDiscoveryMethodsViaReflection();
            
            _initialized = true;
        }
    }
    
    /// <summary>
    /// Uses reflection to discover all IDiscoveryMethod implementations and loads them.
    /// </summary>
    private static void LoadDiscoveryMethodsViaReflection()
    {
        var discoveredMethods = new List<(string Id, IDiscoveryMethod Instance, string Assembly)>();
        
        // Get all loaded assemblies to search for IDiscoveryMethod implementations
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !IsSystemAssembly(assembly.GetName().Name))
            .ToList();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var discoveryMethodTypes = assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && 
                                   typeof(IDiscoveryMethod).IsAssignableFrom(type) &&
                                   type.GetConstructor(Type.EmptyTypes) != null) // Has parameterless constructor
                    .ToList();
                
                foreach (var methodType in discoveryMethodTypes)
                {
                    try
                    {
                        var instance = (IDiscoveryMethod)Activator.CreateInstance(methodType)!;
                        discoveredMethods.Add((instance.Identifier, instance, assembly.GetName().Name ?? "Unknown"));
                        
                        // Use TryAdd to avoid issues with duplicate identifiers
                        _discoveredMethods.TryAdd(instance.Identifier, instance);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail - continue with other methods
                        System.Diagnostics.Debug.WriteLine($"Failed to instantiate discovery method {methodType.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - continue with other assemblies
                System.Diagnostics.Debug.WriteLine($"Failed to scan assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }
        
        // Debug output to show what was discovered
        System.Diagnostics.Debug.WriteLine($"Auto-discovered {discoveredMethods.Count} discovery methods:");
        foreach (var (id, _, assemblyName) in discoveredMethods.OrderBy(m => m.Id))
        {
            System.Diagnostics.Debug.WriteLine($"  - {id} (from {assemblyName})");
        }
    }
    
    /// <summary>
    /// Executes the configured discovery methods and returns results.
    /// </summary>
    private static DiscoveryExecutionResult ExecuteDiscoveryMethods(string[] configuredMethods, Compilation compilation)
    {
        var resolved = new List<IDiscoveryMethod>();
        var warnings = new List<string>();
        var errors = new List<string>();
        
        // Resolve configured methods
        foreach (var methodId in configuredMethods ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(methodId))
            {
                warnings.Add("Empty or null discovery method identifier found in configuration");
                continue;
            }
            
            var normalizedId = methodId.Trim();
            
            if (_discoveredMethods.TryGetValue(normalizedId, out var method))
            {
                if (!method.CanRun(compilation))
                {
                    warnings.Add($"Discovery method '{normalizedId}' cannot run in current context - skipping");
                    continue;
                }
                
                resolved.Add(method);
            }
            else
            {
                errors.Add($"Unknown discovery method '{normalizedId}' - available methods: {string.Join(", ", _discoveredMethods.Keys)}");
            }
        }
        
        if (errors.Count > 0)
        {
            return new DiscoveryExecutionResult
            {
                DiscoveredTypes = new List<DiscoveredType>(),
                Warnings = warnings,
                Errors = errors,
                Success = false
            };
        }
        
        // Execute discovery methods
        var allDiscoveredTypes = new List<DiscoveredType>();
        var executionWarnings = new List<string>(warnings);
        
        foreach (var method in resolved)
        {
            try
            {
                var discoveredTypes = method.Discover(compilation);
                allDiscoveredTypes.AddRange(discoveredTypes);
            }
            catch (Exception ex)
            {
                executionWarnings.Add($"Discovery method '{method.Identifier}' failed: {ex.Message}");
            }
        }
        
        // Aggregate types found by multiple discovery methods
        var aggregated = allDiscoveredTypes
            .GroupBy(dt => dt.TypeSymbol, SymbolEqualityComparer.Default)
            .Select(group => AggregateDiscoveredType(group))
            .ToList();
        
        return new DiscoveryExecutionResult
        {
            DiscoveredTypes = aggregated,
            Warnings = executionWarnings,
            Errors = errors,
            Success = true,
            MethodsUsed = resolved.Select(m => m.Identifier).ToArray()
        };
    }
    
    /// <summary>
    /// Aggregates multiple DiscoveredType instances for the same type symbol into a single instance.
    /// </summary>
    /// <param name="discoveredTypes">Group of DiscoveredType instances for the same type.</param>
    /// <returns>Single aggregated DiscoveredType with all discovery methods.</returns>
    private static DiscoveredType AggregateDiscoveredType(IEnumerable<DiscoveredType> discoveredTypes)
    {
        var typeList = discoveredTypes.ToList();
        var firstType = typeList.First();
        
        // Prefer syntax discovery as the primary source
        var primarySource = typeList.Any(dt => dt.Source == DiscoverySource.Syntax) 
            ? DiscoverySource.Syntax 
            : DiscoverySource.Referenced;
        
        // Aggregate all discovery methods and contexts
        var allDiscoveredBy = typeList.SelectMany(dt => dt.DiscoveredBy ?? new string[0]).Distinct().ToArray();
        var allContexts = new Dictionary<string, string?>();
        
        foreach (var discoveredType in typeList)
        {
            foreach (var kvp in discoveredType.DiscoveryContexts)
            {
                if (!string.IsNullOrEmpty(kvp.Key))
                {
                    allContexts[kvp.Key] = kvp.Value;
                }
            }
        }
        
        return new DiscoveredType
        {
            TypeSymbol = firstType.TypeSymbol,
            Source = primarySource,
            DiscoveredBy = allDiscoveredBy,
            DiscoveryContexts = allContexts
        };
    }
    
    /// <summary>
    /// Checks if an assembly is a system assembly that should be skipped during discovery.
    /// </summary>
    private static bool IsSystemAssembly(string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return true;
            
        return assemblyName!.StartsWith("System.") ||
               assemblyName.StartsWith("Microsoft.") ||
               assemblyName.StartsWith("mscorlib") ||
               assemblyName.StartsWith("netstandard") ||
               assemblyName.StartsWith("Microsoft.CodeAnalysis");
    }
}

/// <summary>
/// Result of executing discovery methods.
/// </summary>
public class DiscoveryExecutionResult
{
    public List<DiscoveredType> DiscoveredTypes { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string[] MethodsUsed { get; set; } = new string[0];
}