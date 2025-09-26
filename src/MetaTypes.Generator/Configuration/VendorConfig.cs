using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MetaTypes.Generator.Common.Configuration
{
    /// <summary>
    /// Configuration for vendor extensions
    /// </summary>
    public class VendorConfig
    {
        /// <summary>
        /// EfCore vendor configuration
        /// </summary>
        [JsonPropertyName("EfCore")]
        public EfCoreVendorConfig? EfCore { get; set; }
        
        /// <summary>
        /// Additional vendor configurations can be added here
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? Extensions { get; set; }
    }
    
    /// <summary>
    /// EfCore-specific vendor configuration
    /// </summary>
    public class EfCoreVendorConfig
    {
        /// <summary>
        /// Whether to require base MetaTypes to be generated before EfCore extensions
        /// </summary>
        [JsonPropertyName("RequireBaseTypes")]
        public bool RequireBaseTypes { get; set; } = true;
        
        /// <summary>
        /// Whether to generate navigation property metadata
        /// </summary>
        [JsonPropertyName("IncludeNavigationProperties")]
        public bool IncludeNavigationProperties { get; set; } = true;
        
        /// <summary>
        /// Whether to generate foreign key relationships
        /// </summary>
        [JsonPropertyName("IncludeForeignKeys")]
        public bool IncludeForeignKeys { get; set; } = true;
    }
}