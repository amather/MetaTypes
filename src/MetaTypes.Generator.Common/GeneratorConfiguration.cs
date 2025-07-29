using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Root configuration object for MetaTypes generators
/// </summary>
public class MetaTypesGeneratorConfiguration
{
    [JsonPropertyName("MetaTypes.Generator")]
    public BaseGeneratorOptions? BaseGenerator { get; set; }
    
    [JsonPropertyName("MetaTypes.Generator.EfCore")]
    public EfCoreGeneratorOptions? EfCoreGenerator { get; set; }
}

/// <summary>
/// Configuration options for the base MetaTypes generator
/// </summary>
public class BaseGeneratorOptions
{
    [JsonPropertyName("EnableEfCoreDetection")]
    public bool EnableEfCoreDetection { get; set; } = true;
    
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    [JsonPropertyName("AssemblyName")]
    public string? AssemblyName { get; set; }
    
    public string? DebugInfo { get; set; }
}

/// <summary>
/// Configuration options for the EfCore MetaTypes generator
/// </summary>
public class EfCoreGeneratorOptions
{
    [JsonPropertyName("EnableBaseDetection")]
    public bool EnableBaseDetection { get; set; } = true;
    
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    [JsonPropertyName("AssemblyName")]
    public string? AssemblyName { get; set; }
    
    public string? DebugInfo { get; set; }
}

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
            return options.TryGetValue("build_metadata.AdditionalFiles.Type", out var type) &&
                   type == "MetaTypes.Generator.Options";
        });

        if (configFile != null)
        {
            var content = configFile.GetText()?.ToString();
            var config = ParseConfiguration(content);
            if (config != null)
            {
                // Add debug info showing successful load
                if (config.BaseGenerator != null)
                    config.BaseGenerator.DebugInfo = $"JSON_CONFIG_LOADED_FROM_{configFile.Path}";
                if (config.EfCoreGenerator != null)
                    config.EfCoreGenerator.DebugInfo = $"JSON_CONFIG_LOADED_FROM_{configFile.Path}";
                return config;
            }
        }

        // Fallback to default configuration
        var defaultConfig = GetDefaultConfiguration();
        if (defaultConfig.BaseGenerator != null)
            defaultConfig.BaseGenerator.DebugInfo = "JSON_CONFIG_NOT_FOUND_USING_DEFAULTS";
        if (defaultConfig.EfCoreGenerator != null)
            defaultConfig.EfCoreGenerator.DebugInfo = "JSON_CONFIG_NOT_FOUND_USING_DEFAULTS";
        return defaultConfig;
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
            
            return JsonSerializer.Deserialize<MetaTypesGeneratorConfiguration>(jsonContent!, options);
        }
        catch
        {
            // Return null if parsing fails - generators should handle gracefully
            return null;
        }
    }
    
    /// <summary>
    /// Gets default configuration when no config file is found
    /// </summary>
    public static MetaTypesGeneratorConfiguration GetDefaultConfiguration()
    {
        return new MetaTypesGeneratorConfiguration
        {
            BaseGenerator = new BaseGeneratorOptions
            {
                EnableEfCoreDetection = true,
                EnableDiagnosticFiles = true
            },
            EfCoreGenerator = new EfCoreGeneratorOptions
            {
                EnableBaseDetection = true,
                EnableDiagnosticFiles = true
            }
        };
    }

}