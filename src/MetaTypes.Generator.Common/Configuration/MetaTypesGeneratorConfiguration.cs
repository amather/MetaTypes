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
    
    [JsonPropertyName("MetaTypes.Generator")]
    public BaseGeneratorOptions? BaseGenerator { get; set; }
}