using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Root configuration object for MetaTypes generators
/// </summary>
public class MetaTypesGeneratorConfiguration
{
    /// <summary>
    /// Top-level generated namespace shared by all generators
    /// </summary>
    [JsonPropertyName("GeneratedNamespace")]
    public string? GeneratedNamespace { get; set; }
    

    // BaseGeneratorOptions

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



    // DiscoveryMethodsConfig

    [JsonPropertyName("DiscoverCrossAssembly")]
    public bool DiscoverCrossAssembly { get; set; } = false;

    [JsonPropertyName("DiscoverMethods")]
    public List<string> DiscoverMethods { get; set; } = [ "MetaTypes.Attribute", "MetaTypes.Reference" ];


    // GenerationConfig

    [JsonPropertyName("GenerateBaseMetaTypes")]
    public bool GenerateBaseMetaTypes { get; set; } = false;
}