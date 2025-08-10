using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MetaTypes.Generator.Common.Configuration;

namespace MetaTypes.Generator.Common.Generator
{
    /// <summary>
    /// Registry for discovering and managing vendor generators via reflection
    /// </summary>
    public static class VendorGeneratorRegistry
    {
        private static List<IVendorGenerator>? _vendorGenerators;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets all available vendor generators discovered via reflection
        /// </summary>
        public static IReadOnlyList<IVendorGenerator> GetVendorGenerators()
        {
            if (_vendorGenerators == null)
            {
                lock (_lock)
                {
                    if (_vendorGenerators == null)
                    {
                        _vendorGenerators = DiscoverVendorGenerators();
                    }
                }
            }
            return _vendorGenerators;
        }

        /// <summary>
        /// Gets vendor generators that are enabled based on configuration
        /// </summary>
        public static IEnumerable<IVendorGenerator> GetEnabledVendorGenerators(VendorConfig? vendorConfig)
        {
            var allGenerators = GetVendorGenerators();
            
            if (vendorConfig == null)
            {
                // No vendor configuration, return no generators
                return Enumerable.Empty<IVendorGenerator>();
            }

            var enabledGenerators = new List<IVendorGenerator>();

            foreach (var generator in allGenerators)
            {
                // Check if this vendor is configured
                if (IsVendorEnabled(generator.VendorName, vendorConfig))
                {
                    enabledGenerators.Add(generator);
                }
            }

            return enabledGenerators;
        }

        private static bool IsVendorEnabled(string vendorName, VendorConfig vendorConfig)
        {
            // Check specific vendor configurations
            switch (vendorName.ToLowerInvariant())
            {
                case "efcore":
                    return vendorConfig.EfCore != null;
                default:
                    // Check if vendor exists in extensions
                    return vendorConfig.Extensions?.ContainsKey(vendorName) ?? false;
            }
        }

        private static List<IVendorGenerator> DiscoverVendorGenerators()
        {
            var generators = new List<IVendorGenerator>();
            
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                // Find all types that implement IVendorGenerator
                var vendorGeneratorTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && 
                               !t.IsInterface && 
                               typeof(IVendorGenerator).IsAssignableFrom(t))
                    .ToList();

                foreach (var generatorType in vendorGeneratorTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(generatorType) as IVendorGenerator;
                        if (instance != null)
                        {
                            generators.Add(instance);
                        }
                    }
                    catch
                    {
                        // Skip generators that can't be instantiated
                    }
                }
            }
            catch
            {
                // If reflection fails, return empty list
            }

            return generators;
        }
    }
}