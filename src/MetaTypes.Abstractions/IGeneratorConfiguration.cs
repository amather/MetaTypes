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