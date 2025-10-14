using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MetaTypes.Generator.Configuration;
using MetaTypes.Generator.Diagnostics;

namespace MetaTypes.Generator;

/// <summary>
/// Main Roslyn diagnostic analyzer for MetaTypes.
/// Reads metatypes.config.json and delegates to appropriate IDiagnosticAnalyzerProvider implementations
/// based on enabled discovery methods.
///
/// Note: SupportedDiagnostics is called BEFORE Initialize() by the Roslyn infrastructure,
/// so all providers are discovered via reflection at static initialization time.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MetaTypesDiagnosticAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            // This property is called BEFORE Initialize() by Roslyn infrastructure
            // Aggregate all supported diagnostics from all discovered vendor providers
            var diagnostics = new List<DiagnosticDescriptor>();

            var allProviders = DiagnosticProviderRegistry.GetAllProviders();
            foreach (var provider in allProviders)
            {
                diagnostics.AddRange(provider.SupportedDiagnostics);
            }

            return diagnostics.ToImmutableArray();
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for named type symbol analysis
        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Try to read metatypes.config.json to determine which providers are enabled
            var enabledProviders = GetEnabledProviders(compilationContext.Options);

            if (!enabledProviders.Any())
            {
                // No providers enabled, don't register analysis
                return;
            }

            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                if (symbolContext.Symbol is not INamedTypeSymbol namedType)
                    return;

                // Analyze the type with each enabled provider
                foreach (var provider in enabledProviders)
                {
                    try
                    {
                        // Get attributes that might be relevant to this provider
                        var attributes = namedType.GetAttributes();

                        // Let the provider decide which attributes are relevant
                        provider.Analyze(symbolContext, namedType, attributes);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't crash the analyzer
                        // In production, we might want better error handling
                        System.Diagnostics.Debug.WriteLine($"Error in diagnostic provider {provider.Identifier}: {ex}");
                    }
                }
            }, SymbolKind.NamedType);
        });
    }

    private List<IDiagnosticAnalyzerProvider> GetEnabledProviders(AnalyzerOptions analyzerOptions)
    {
        try
        {
            // Try to find metatypes.config.json using the same approach as the generator
            var configFile = FindConfigFile(analyzerOptions);
            if (configFile == null)
            {
                // No config file, no providers enabled
                return new List<IDiagnosticAnalyzerProvider>();
            }

            // Read config content using the approved API (not File.ReadAllText)
            var configContent = configFile.GetText()?.ToString();
            if (string.IsNullOrWhiteSpace(configContent))
            {
                return new List<IDiagnosticAnalyzerProvider>();
            }

            // Parse config using the same parser as the generator
            var options = ConfigurationLoader.ParseConfiguration(configContent);

            if (options?.DiscoverMethods == null || options.DiscoverMethods.Count == 0)
            {
                return new List<IDiagnosticAnalyzerProvider>();
            }

            // Use registry to get providers that match the enabled discovery methods
            return DiagnosticProviderRegistry.GetEnabledProviders(options.DiscoverMethods).ToList();
        }
        catch (Exception ex)
        {
            // Failed to read config, disable all providers
            System.Diagnostics.Debug.WriteLine($"Error reading metatypes.config.json: {ex}");
            return new List<IDiagnosticAnalyzerProvider>();
        }
    }

    private AdditionalText? FindConfigFile(AnalyzerOptions analyzerOptions)
    {
        // Use the same approach as the generator to find metatypes.config.json
        // Look for an AdditionalFile with Type metadata set to "MetaTypes.Generator.Options"
        var configFile = analyzerOptions.AdditionalFiles.FirstOrDefault(file =>
        {
            var options = analyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(file);
            return options.TryGetValue("build_metadata.AdditionalFiles.Type", out var type) &&
                   type == "MetaTypes.Generator.Options";
        });

        return configFile;
    }
}
