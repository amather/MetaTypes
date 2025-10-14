using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MetaTypes.Generator.Diagnostics;

/// <summary>
/// Registry for discovering and managing diagnostic analyzer providers via reflection
/// </summary>
public static class DiagnosticProviderRegistry
{
    private static List<IDiagnosticAnalyzerProvider>? _providers;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets all available diagnostic providers discovered via reflection
    /// </summary>
    public static IReadOnlyList<IDiagnosticAnalyzerProvider> GetAllProviders()
    {
        if (_providers == null)
        {
            lock (_lock)
            {
                if (_providers == null)
                {
                    _providers = DiscoverProviders();
                }
            }
        }
        return _providers;
    }

    /// <summary>
    /// Gets available provider identifiers for diagnostics
    /// </summary>
    public static IEnumerable<string> GetAvailableProviderIdentifiers()
    {
        return GetAllProviders().Select(p => p.Identifier);
    }

    /// <summary>
    /// Gets diagnostic providers that match the enabled discovery methods
    /// </summary>
    public static IEnumerable<IDiagnosticAnalyzerProvider> GetEnabledProviders(List<string>? discoveryMethods)
    {
        var allProviders = GetAllProviders();

        if (discoveryMethods == null || discoveryMethods.Count == 0)
        {
            // No discovery methods enabled, no providers active
            return Array.Empty<IDiagnosticAnalyzerProvider>();
        }

        var enabledProviders = new List<IDiagnosticAnalyzerProvider>();

        foreach (var provider in allProviders)
        {
            // Check if this provider's identifier matches any enabled discovery method
            if (discoveryMethods.Contains(provider.Identifier))
            {
                enabledProviders.Add(provider);
            }
        }

        return enabledProviders;
    }

    private static List<IDiagnosticAnalyzerProvider> DiscoverProviders()
    {
        var providers = new List<IDiagnosticAnalyzerProvider>();

        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Find all types that implement IDiagnosticAnalyzerProvider
            var providerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           typeof(IDiagnosticAnalyzerProvider).IsAssignableFrom(t))
                .ToList();

            foreach (var providerType in providerTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(providerType) as IDiagnosticAnalyzerProvider;
                    if (instance != null)
                    {
                        providers.Add(instance);
                    }
                }
                catch
                {
                    // Skip providers that can't be instantiated
                }
            }
        }
        catch
        {
            // If reflection fails, return empty list
        }

        return providers;
    }
}
