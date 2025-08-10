using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Base generator configuration section
/// </summary>
public class GeneratorConfigSection : IGeneratorConfigSection
{
    [JsonPropertyName("Discovery")]
    public DiscoveryConfig Discovery { get; set; } = new();
    
    [JsonPropertyName("Generation")]
    public GenerationConfig Generation { get; set; } = new();
    
    [JsonPropertyName("GeneratedNamespace")]
    public string? GeneratedNamespace { get; set; }
    
    IDiscoveryConfig IGeneratorConfigSection.Discovery => Discovery;
    IGenerationConfig IGeneratorConfigSection.Generation => Generation;
}