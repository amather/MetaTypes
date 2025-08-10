using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Configuration for generation options
/// </summary>
public class GenerationConfig : IGenerationConfig
{
    [JsonPropertyName("BaseMetaTypes")]
    public bool BaseMetaTypes { get; set; } = false; // Default: false
}