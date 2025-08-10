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
                // Merge top-level AssemblyName into generator sections if they don't have their own
                MergeAssemblyNames(config);
                
                // Add debug info showing successful load
                if (config.BaseGenerator != null)
                    config.BaseGenerator.DebugInfo = $"JSON_CONFIG_LOADED_FROM_{configFile.Path}";
                return config;
            }
        }

        // Fallback to default configuration
        var defaultConfig = GetDefaultConfiguration();
        if (defaultConfig.BaseGenerator != null)
            defaultConfig.BaseGenerator.DebugInfo = "JSON_CONFIG_NOT_FOUND_USING_DEFAULTS";
        return defaultConfig;
    }
    
    /// <summary>
    /// Loads configuration for a specific generator by name
    /// </summary>
    public static TConfig? LoadGeneratorConfig<TConfig>(
        ImmutableArray<Microsoft.CodeAnalysis.AdditionalText> additionalFiles,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider configProvider,
        string generatorName) where TConfig : class, IGeneratorConfigSection
    {
        var fullConfig = LoadFromAdditionalFiles(additionalFiles, configProvider);
        
        return generatorName switch
        {
            "MetaTypes.Generator" => fullConfig.BaseGenerator as TConfig,
            _ => null
        };
    }
    
    /// <summary>
    /// Merges top-level AssemblyName into generator sections that don't have their own
    /// </summary>
    private static void MergeAssemblyNames(MetaTypesGeneratorConfiguration config)
    {
        if (string.IsNullOrEmpty(config.AssemblyName)) return;
        
        if (config.BaseGenerator?.AssemblyName == null)
            config.BaseGenerator!.AssemblyName = config.AssemblyName;
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
            if (parsedConfig == null)
                return null;
                
            // Merge with defaults to ensure all required properties are set
            var defaultConfig = GetDefaultConfiguration();
            return MergeConfigurations(defaultConfig, parsedConfig);
        }
        catch
        {
            // Return null if parsing fails - generators should handle gracefully
            return null;
        }
    }
    
    /// <summary>
    /// Merges parsed configuration with defaults, preferring values from parsed config
    /// </summary>
    private static MetaTypesGeneratorConfiguration MergeConfigurations(
        MetaTypesGeneratorConfiguration defaultConfig,
        MetaTypesGeneratorConfiguration parsedConfig)
    {
        // Merge BaseGenerator settings
        if (parsedConfig.BaseGenerator != null && defaultConfig.BaseGenerator != null)
        {
            // Preserve parsed values, use defaults for missing ones
            if (parsedConfig.BaseGenerator.Discovery == null)
                parsedConfig.BaseGenerator.Discovery = defaultConfig.BaseGenerator.Discovery;
            if (parsedConfig.BaseGenerator.Generation == null)
                parsedConfig.BaseGenerator.Generation = defaultConfig.BaseGenerator.Generation;
        }
        else if (parsedConfig.BaseGenerator == null)
        {
            parsedConfig.BaseGenerator = defaultConfig.BaseGenerator;
        }
        
        return parsedConfig;
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
                EnableDiagnosticFiles = true,
                
                // New orchestrated configuration - generators should NOT generate base types by default
                // Each generator must explicitly opt-in if they want to generate base types
                Generation = new GenerationConfig { BaseMetaTypes = false },
                Discovery = new DiscoveryConfig 
                { 
                    Syntax = true, 
                    CrossAssembly = false,  // Default to syntax-only, require explicit opt-in for cross-assembly
                    Methods = new DiscoveryMethodsConfig
                    {
                        Methods = new[] { "MetaTypes.Attribute", "MetaTypes.Reference" }
                    }
                }
            }
        };
    }

}