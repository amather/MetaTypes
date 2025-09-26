using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Configuration for discovery methods using pluggable identifiers
/// </summary>
public class DiscoveryMethodsConfig : IDiscoveryMethodsConfig
{
    [JsonPropertyName("Methods")]
    public string[] Methods { get; set; } = { "MetaTypes.Attribute", "MetaTypes.Reference" };
}