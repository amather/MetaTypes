using System.Collections.Generic;
using System.Text.Json;
using MetaTypes.Generator.Configuration;
using MetaTypes.Generator.Discovery;
using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Generator
{
    /// <summary>
    /// Interface for vendor-specific generators that extend the base MetaTypes with additional functionality
    /// </summary>
    public interface IVendorGenerator
    {
        /// <summary>
        /// The name of the vendor (e.g., "EfCore", "Json", "Validation")
        /// </summary>
        string VendorName { get; }
        
        /// <summary>
        /// Description of what this vendor generator provides
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Configure the vendor generator with its specific configuration.
        /// Vendors are responsible for parsing/binding their own config.
        /// </summary>
        /// <param name="config">Raw JSON config for this vendor, or null if no config provided</param>
        void Configure(JsonElement? config);
        
        /// <summary>
        /// Generates vendor-specific extensions for discovered types
        /// </summary>
        /// <param name="discoveredTypes">All discovered types from the discovery phase</param>
        /// <param name="compilation">The current compilation</param>
        /// <param name="context">Generator context with configuration and services</param>
        /// <returns>Generated source files</returns>
        IEnumerable<GeneratedFile> Generate(
            IEnumerable<DiscoveredType> discoveredTypes,
            Compilation compilation,
            MetaTypesOptions config,
            GeneratorContext context);
    }
    
    
    /// <summary>
    /// Context passed to vendor generators
    /// </summary>
    public class GeneratorContext
    {
        public bool EnableDiagnostics { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();
        public string TargetNamespace { get; set; } = "";
    }
    
    /// <summary>
    /// Vendor-specific configuration
    /// </summary>
    public class VendorConfiguration
    {
        public bool RequireBaseTypes { get; set; } = true;
        public Dictionary<string, object> Settings { get; set; } = new();
    }
}