namespace MetaTypes.Abstractions;

/// <summary>
/// Common interface for generator configuration objects that need assembly name detection.
/// Allows different source generators to share the same assembly name detection logic.
/// </summary>
public interface IGeneratorConfiguration
{
    /// <summary>
    /// Override assembly name for namespace generation.
    /// If null, the assembly name detection logic will be used.
    /// </summary>
    string? AssemblyName { get; }
}