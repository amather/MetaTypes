using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Configuration;

/// <summary>
/// Helper class for loading generator configuration from AnalyzerConfigOptions
/// </summary>
public static class ConfigurationLoader
{
    /// <summary>
    /// Loads configuration from AdditionalFiles (JSON configuration)
    /// </summary>
    public static MetaTypesOptions LoadFromAdditionalFiles(
        ImmutableArray<Microsoft.CodeAnalysis.AdditionalText> additionalFiles,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider configProvider)
    {
        // Find the MetaTypes configuration file:
        //
        // we're looking for an AdditionalFile with a specific metadata key, e.g.:
        //
        // <ItemGroup>
        //   <AdditionalFiles Include="metatypes.config.json" Type="MetaTypes.Generator.Options" />
        //   <CompilerVisibleItemMetadata Include="AdditionalFiles" Metadata="Type" />
        // </ItemGroup>
        //
        // So the project defines that the AdditionalFiles, including its `Type` attribute, is to 
        // be made available/visible to the compiler and therefore the source generator.
        //
        // In here, we detect this and correspondingly load the configuration from that file.

        var configFile = additionalFiles.FirstOrDefault(file =>
        {
            var options = configProvider.GetOptions(file);
            return options.TryGetValue("build_metadata.AdditionalFiles.Type", out var type) && type == "MetaTypes.Generator.Options";
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
        return new MetaTypesOptions();
    }
    

    /// <summary>
    /// Attempts to parse a JSON configuration string into MetaTypesGeneratorConfiguration
    /// </summary>
    public static MetaTypesOptions? ParseConfiguration(string? jsonContent)
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
            
            var parsedConfig = JsonSerializer.Deserialize<MetaTypesOptions>(jsonContent!, options);
            return parsedConfig;
        }
        catch
        {
            return null;
        }
    }
    
}