using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaTypes.Generator.Configuration;

/// <summary>
/// Root configuration object for MetaTypes generators
/// </summary>
public class MetaTypesOptions
{
    [JsonPropertyName("GeneratedNamespace")]
    public string? GeneratedNamespace { get; set; }

    [JsonPropertyName("GenerateBaseMetaTypes")]
    public bool GenerateBaseMetaTypes { get; set; } = false;

    [JsonPropertyName("DiscoverCrossAssembly")]
    public bool DiscoverCrossAssembly { get; set; } = false;

    [JsonPropertyName("DiscoverMethods")]
    public List<string> DiscoverMethods { get; set; } = ["MetaTypes.Attribute", "MetaTypes.Reference"];

    [JsonPropertyName("EnabledVendors")]
    public List<string>? EnabledVendors { get; set; }

    [JsonPropertyName("VendorConfigs")]
    public Dictionary<string, JsonElement>? VendorConfigs { get; set; }

    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;

    public string? DebugInfo { get; set; }
}