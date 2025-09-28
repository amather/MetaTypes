using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;
using MetaTypes.Generator.Configuration;

namespace MetaTypes.Generator.Discovery;

/// <summary>
/// Type discovery system that auto-discovers multiple discovery methods via reflection.
/// </summary>
public static class TypeDiscovery
{
    /// <summary>
    /// Runs the type discovery and returns detailed results including warnings and errors.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="config">Configuration</param>
    /// <returns>Detailed discovery execution result with diagnostics.</returns>
    public static DiscoveryExecutionResult RunTypeDiscovery(Compilation compilation, MetaTypesOptions config)
    {
        // config defines IDiscoveryMethod.Identifier values to run
        var configuredMethods = config.DiscoverMethods;

        // using reflection, find all IDiscoveryMethod implementations
        DiscoveryExecutionResult result = new();
        var discoverMethods = FindDiscoveryMethods(result);

        // filter discovered methods to only those configured to run
        var filteredMethods = discoverMethods.Values
            .Where(x => configuredMethods.Contains(x.Identifier, StringComparer.OrdinalIgnoreCase));

        // if config disables cross-assembly discovery, but discovery method requires it, we exclude the method.
        if (!config.DiscoverCrossAssembly)
        {
            filteredMethods.Where(m => !m.RequiresCrossAssembly);
        }

        // run the actual type discovery methods
        var methodsToRun = filteredMethods.ToList();
        ExecuteDiscoveryMethods(methodsToRun, compilation, result);

        result.Success = result.DiscoveredTypes.Count > 0 && result.Errors.Count == 0;
        return result;
    }


    private static Dictionary<string, IDiscoveryMethod> FindDiscoveryMethods(DiscoveryExecutionResult result)
    {
        Dictionary<string, IDiscoveryMethod> discoveredMethods = [];


        var discoveryMethodTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract &&
                           typeof(IDiscoveryMethod).IsAssignableFrom(type) &&
                           type.GetConstructor(Type.EmptyTypes) != null) // Has parameterless constructor
            .ToList();

        foreach (var methodType in discoveryMethodTypes)
        {
            try
            {
                var instance = (IDiscoveryMethod)Activator.CreateInstance(methodType)!;
                discoveredMethods.Add(instance.Identifier, instance);
                result.Debug.Add($"Discovered method: {instance.Identifier}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to instantiate discovery method {methodType.Name}: {ex.Message}");
            }
        }

        return discoveredMethods;
    }

    private static void ExecuteDiscoveryMethods(List<IDiscoveryMethod> methodsToRun, Compilation compilation, DiscoveryExecutionResult result)
    {
        List<DiscoveredType> allDiscoveredTypes = [];

        result.MethodsUsed = [];
        foreach (var method in methodsToRun)
        {
            try
            {
                var discoveredTypes = method.Discover(compilation);
                allDiscoveredTypes.AddRange(discoveredTypes);

                result.MethodsUsed.Add(method.Identifier);
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Discovery method '{method.Identifier}' failed: {ex.Message}");
            }
        }

        // aggregate types found by multiple discovery methods
        var aggregated = allDiscoveredTypes
            .GroupBy(dt => dt.TypeSymbol, SymbolEqualityComparer.Default)
            .Select(group => AggregateDiscoveredType(group))
            .ToList();

        result.DiscoveredTypes = aggregated;
    }


    private static DiscoveredType AggregateDiscoveredType(IEnumerable<DiscoveredType> discoveredTypes)
    {
        var typeList = discoveredTypes.ToList();
        var firstType = typeList.First();
        
        // prefer syntax discovery as the primary source
        var primarySource = typeList.Any(dt => dt.Source == DiscoverySource.Syntax) 
            ? DiscoverySource.Syntax 
            : DiscoverySource.Referenced;
        
        // aggregate all discovery methods and contexts
        var allDiscoveredBy = typeList.SelectMany(dt => dt.DiscoveredBy).Distinct().ToList();
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
    
}

