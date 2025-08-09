using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

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
    
    [JsonPropertyName("MetaTypes.Generator.EfCore")]
    public EfCoreGeneratorOptions? EfCoreGenerator { get; set; }
}

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
    public DiscoveryMethodsConfig Methods { get; set; } = new();
    
    IDiscoveryMethodsConfig IDiscoveryConfig.Methods => Methods;
}

/// <summary>
/// Configuration for discovery methods
/// </summary>
public class DiscoveryMethodsConfig : IDiscoveryMethodsConfig
{
    [JsonPropertyName("MetaTypesAttributes")]
    public bool MetaTypesAttributes { get; set; } = true;
    
    [JsonPropertyName("MetaTypesReferences")]
    public bool MetaTypesReferences { get; set; } = true;
}

/// <summary>
/// Configuration for generation options
/// </summary>
public class GenerationConfig : IGenerationConfig
{
    [JsonPropertyName("BaseMetaTypes")]
    public bool BaseMetaTypes { get; set; } = false; // Default: false
}

/// <summary>
/// Base generator configuration section
/// </summary>
public class GeneratorConfigSection : IGeneratorConfigSection
{
    [JsonPropertyName("Discovery")]
    public DiscoveryConfig Discovery { get; set; } = new();
    
    [JsonPropertyName("Generation")]
    public GenerationConfig Generation { get; set; } = new();
    
    [JsonPropertyName("AssemblyName")]
    public string? AssemblyName { get; set; }
    
    IDiscoveryConfig IGeneratorConfigSection.Discovery => Discovery;
    IGenerationConfig IGeneratorConfigSection.Generation => Generation;
}

/// <summary>
/// Configuration options for the base MetaTypes generator
/// </summary>
public class BaseGeneratorOptions : GeneratorConfigSection
{
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    // Legacy support - will be removed in future versions
    [JsonPropertyName("EnableEfCoreDetection")]
    public bool EnableEfCoreDetection { get; set; } = true;
    
    public string? DebugInfo { get; set; }
}

/// <summary>
/// Configuration options for the EfCore MetaTypes generator  
/// </summary>
public class EfCoreGeneratorOptions : GeneratorConfigSection
{
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    // Legacy support - will be removed in future versions
    [JsonPropertyName("EnableBaseDetection")]
    public bool EnableBaseDetection { get; set; } = true;
    
    [JsonPropertyName("EfCore")]
    public EfCoreSpecificConfig? EfCore { get; set; }
    
    public string? DebugInfo { get; set; }
}

/// <summary>
/// EF Core specific configuration extensions
/// </summary>
public class EfCoreSpecificConfig
{
    [JsonPropertyName("RequireBaseTypes")]
    public bool RequireBaseTypes { get; set; } = true;
}

/// <summary>
/// Extended discovery methods for EF Core
/// </summary>
public class EfCoreDiscoveryMethodsConfig : DiscoveryMethodsConfig
{
    [JsonPropertyName("EfCoreEntities")]
    public bool EfCoreEntities { get; set; } = true;
    
    [JsonPropertyName("DbContextScanning")]  
    public bool DbContextScanning { get; set; } = true;
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
            "MetaTypes.Generator.EfCore" => fullConfig.EfCoreGenerator as TConfig,
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
            
        if (config.EfCoreGenerator?.AssemblyName == null) 
            config.EfCoreGenerator!.AssemblyName = config.AssemblyName;
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
        
        // Merge EfCoreGenerator settings  
        if (parsedConfig.EfCoreGenerator != null && defaultConfig.EfCoreGenerator != null)
        {
            // Preserve parsed values, use defaults for missing ones
            if (parsedConfig.EfCoreGenerator.Discovery == null)
            {
                parsedConfig.EfCoreGenerator.Discovery = defaultConfig.EfCoreGenerator.Discovery;
            }
            else if (!(parsedConfig.EfCoreGenerator.Discovery.Methods is EfCoreDiscoveryMethodsConfig))
            {
                // If Methods was not parsed as EfCoreDiscoveryMethodsConfig, use the default
                // This happens because JSON deserializer doesn't know to use EfCoreDiscoveryMethodsConfig
                parsedConfig.EfCoreGenerator.Discovery.Methods = defaultConfig.EfCoreGenerator.Discovery.Methods;
            }
            
            if (parsedConfig.EfCoreGenerator.Generation == null)
                parsedConfig.EfCoreGenerator.Generation = defaultConfig.EfCoreGenerator.Generation;
            if (parsedConfig.EfCoreGenerator.EfCore == null)
                parsedConfig.EfCoreGenerator.EfCore = defaultConfig.EfCoreGenerator.EfCore;
        }
        else if (parsedConfig.EfCoreGenerator == null)
        {
            parsedConfig.EfCoreGenerator = defaultConfig.EfCoreGenerator;
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
                // Legacy compatibility
                EnableEfCoreDetection = true,
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
                        MetaTypesAttributes = true,
                        MetaTypesReferences = true
                    }
                }
            },
            EfCoreGenerator = new EfCoreGeneratorOptions
            {
                // Legacy compatibility
                EnableBaseDetection = true,
                EnableDiagnosticFiles = true,
                
                // New orchestrated configuration - generators should NOT generate base types by default
                Generation = new GenerationConfig { BaseMetaTypes = false },
                Discovery = new DiscoveryConfig 
                { 
                    Syntax = true, 
                    CrossAssembly = false, // Default to syntax-only, require explicit opt-in for cross-assembly
                    Methods = new EfCoreDiscoveryMethodsConfig
                    {
                        MetaTypesAttributes = true,
                        MetaTypesReferences = false, // Let base generator handle this
                        EfCoreEntities = true,
                        DbContextScanning = true
                    }
                },
                EfCore = new EfCoreSpecificConfig
                {
                    RequireBaseTypes = true
                }
            }
        };
    }

}