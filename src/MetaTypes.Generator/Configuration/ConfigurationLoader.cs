using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Helper class for loading generator configuration from AnalyzerConfigOptions
/// </summary>
public static class ConfigurationLoader
{
    /// <summary>
    /// Loads configuration from AdditionalFiles (JSON configuration)
    /// </summary>
    public static MetaTypesGeneratorConfiguration LoadFromAdditionalFiles(
        ImmutableArray<Microsoft.CodeAnalysis.AdditionalText> additionalFiles,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider configProvider)
    {
        // Find the MetaTypes configuration file
        var configFile = additionalFiles.FirstOrDefault(file =>
        {
            var options = configProvider.GetOptions(file);
            return (options.TryGetValue("build_metadata.AdditionalFiles.GeneratorConfiguration", out var type) ||
                    options.TryGetValue("build_metadata.AdditionalFiles.Type", out type)) &&
                   type == "MetaTypes.Generator.Options";
        });

        if (configFile != null)
        {
            var content = configFile.GetText()?.ToString();
            var config = ParseConfiguration(content);
            if (config != null)
            {
                config.DebugInfo = $"JSON_CONFIG_LOADED_FROM_{configFile.Path}";
                return config;
            }
        }

        // Fallback to default configuration
        return new MetaTypesGeneratorConfiguration();
    }
    

    /// <summary>
    /// Attempts to parse a JSON configuration string into MetaTypesGeneratorConfiguration
    /// </summary>
    public static MetaTypesGeneratorConfiguration? ParseConfiguration(string? jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return null;
            
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            var parsedConfig = JsonSerializer.Deserialize<MetaTypesGeneratorConfiguration>(jsonContent!, options);
            return parsedConfig;
        }
        catch
        {
            // Return null if parsing fails - generators should handle gracefully
            return null;
        }
    }
    
}