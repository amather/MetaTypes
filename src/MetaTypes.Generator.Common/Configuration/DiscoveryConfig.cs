using System.Text.Json.Serialization;
using System.Text.Json;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Configuration for type discovery options
/// </summary>
public class DiscoveryConfig : IDiscoveryConfig
{
    [JsonPropertyName("Syntax")]
    public bool Syntax { get; set; } = true;
    
    [JsonPropertyName("CrossAssembly")]
    public bool CrossAssembly { get; set; } = false;
    
    [JsonPropertyName("Methods")]
    [JsonConverter(typeof(DiscoveryMethodsConverter))]
    public DiscoveryMethodsConfig Methods { get; set; } = new();
    
    IDiscoveryMethodsConfig IDiscoveryConfig.Methods => Methods;
}

/// <summary>
/// Custom JSON converter that handles both direct string arrays and nested objects for Methods
/// </summary>
public class DiscoveryMethodsConverter : JsonConverter<DiscoveryMethodsConfig>
{
    public override DiscoveryMethodsConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Direct array format: "Methods": ["method1", "method2"]
            var methods = JsonSerializer.Deserialize<string[]>(ref reader, options) ?? Array.Empty<string>();
            return new DiscoveryMethodsConfig { Methods = methods };
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Nested object format: "Methods": { "Methods": ["method1", "method2"] }
            var nestedObject = JsonSerializer.Deserialize<DiscoveryMethodsConfig>(ref reader, options);
            return nestedObject ?? new DiscoveryMethodsConfig();
        }
        else
        {
            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DiscoveryMethodsConfig");
        }
    }

    public override void Write(Utf8JsonWriter writer, DiscoveryMethodsConfig value, JsonSerializerOptions options)
    {
        // Always write as direct array format
        JsonSerializer.Serialize(writer, value.Methods, options);
    }
}