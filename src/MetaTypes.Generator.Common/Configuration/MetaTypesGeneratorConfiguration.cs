using System.Text.Json.Serialization;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Root configuration object for MetaTypes generators
/// </summary>
public class MetaTypesGeneratorConfiguration
{
    /// <summary>
    /// Top-level assembly name shared by all generators
    /// </summary>
    [JsonPropertyName("AssemblyName")]
    public string? AssemblyName { get; set; }
    
    [JsonPropertyName("MetaTypes.Generator")]
    public BaseGeneratorOptions? BaseGenerator { get; set; }
}