namespace MetaTypes.Abstractions;

/// <summary>
/// Interface for generator configurations that support assembly name overrides.
/// This allows generators to use MetaTypes' superior assembly name logic.
/// </summary>
public interface IGeneratorConfiguration
{
    /// <summary>
    /// Optional assembly name override. If provided, this will be used as the target namespace
    /// instead of the actual assembly name.
    /// </summary>
    string? AssemblyName { get; }
}

/// <summary>
/// Configuration for type discovery options
/// </summary>
public interface IDiscoveryConfig
{
    /// <summary>
    /// Enable syntax-based discovery (default: true)
    /// </summary>
    bool Syntax { get; }
    
    /// <summary>
    /// Enable cross-assembly discovery (default: false)
    /// </summary>
    bool CrossAssembly { get; }
    
    /// <summary>
    /// Discovery methods configuration
    /// </summary>
    IDiscoveryMethodsConfig Methods { get; }
}

/// <summary>
/// Configuration for discovery methods
/// </summary>
public interface IDiscoveryMethodsConfig
{
    /// <summary>
    /// Discover types with [MetaType] attributes (default: true)
    /// </summary>
    bool MetaTypesAttributes { get; }
    
    /// <summary>
    /// Discover types referenced by other MetaTypes (default: true)
    /// </summary>
    bool MetaTypesReferences { get; }
}

/// <summary>
/// Configuration for generation options
/// </summary>
public interface IGenerationConfig
{
    /// <summary>
    /// Generate base MetaType classes (default: false)
    /// Most generators should extend existing base classes rather than create new ones
    /// </summary>
    bool BaseMetaTypes { get; }
}

/// <summary>
/// Complete generator configuration section
/// </summary>
public interface IGeneratorConfigSection : IGeneratorConfiguration
{
    /// <summary>
    /// Type discovery configuration
    /// </summary>
    IDiscoveryConfig Discovery { get; }
    
    /// <summary>
    /// Generation configuration  
    /// </summary>
    IGenerationConfig Generation { get; }
}