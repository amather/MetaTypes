using System.Text.Json.Serialization;
using MetaTypes.Generator.Common.Configuration;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Configuration options for the base MetaTypes generator
/// </summary>
public class BaseGeneratorOptions : GeneratorConfigSection
{
    [JsonPropertyName("EnableDiagnosticFiles")]
    public bool EnableDiagnosticFiles { get; set; } = true;
    
    /// <summary>
    /// Vendor-specific configurations
    /// </summary>
    [JsonPropertyName("Vendors")]
    public VendorConfig? Vendors { get; set; }
    
    public string? DebugInfo { get; set; }
}