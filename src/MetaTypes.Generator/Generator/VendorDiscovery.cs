using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MetaTypes.Generator.Generator;

internal static class VendorDiscovery
{


    internal static List<IVendorGenerator> DiscoverVendorGenerators()
    {
        var generators = new List<IVendorGenerator>();
        var assembly = Assembly.GetExecutingAssembly();

        try
        {
            // find all types that implement IVendorGenerator
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
                    // skip generators that can't be instantiated
                }
            }
        }
        catch
        {
        }

        return generators;
    }
}

