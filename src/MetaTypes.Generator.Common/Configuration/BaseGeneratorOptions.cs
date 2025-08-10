using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetaTypes.Generator.Common.Configuration;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Configuration options for the base MetaTypes generator
/// </summary>
public class BaseGeneratorOptions : GeneratorConfigSection
{
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    /// <summary>
    /// Array of enabled vendor names
    /// </summary>
    [JsonPropertyName("EnabledVendors")]
    public string[]? EnabledVendors { get; set; }
    
    /// <summary>
    /// Vendor-specific configurations (raw JSON elements)
    /// </summary>
    [JsonPropertyName("VendorConfigs")]
    public Dictionary<string, JsonElement>? VendorConfigs { get; set; }
    
    public string? DebugInfo { get; set; }
}