using Microsoft.CodeAnalysis;

namespace MetaTypes.Abstractions;

/// <summary>
/// Provides standardized assembly name detection and namespace generation logic.
/// This interface allows different source generators to share the same proven
/// assembly name detection logic used by MetaTypes.
/// </summary>
public interface IAssemblyNameProvider
{
    /// <summary>
    /// Gets the target namespace for generated code based on compilation and configuration.
    /// </summary>
    /// <param name="compilation">The compilation context</param>
    /// <param name="config">Generator configuration that may override assembly name</param>
    /// <returns>The target namespace to use for generated code</returns>
    string GetTargetNamespace(Compilation compilation, IGeneratorConfiguration config);
    
    /// <summary>
    /// Gets the raw assembly name from compilation.
    /// </summary>
    /// <param name="compilation">The compilation context</param>
    /// <returns>The assembly name from compilation</returns>
    string GetAssemblyName(Compilation compilation);
    
    /// <summary>
    /// Gets the target namespace for a specific assembly, considering configuration overrides.
    /// </summary>
    /// <param name="assemblyName">The raw assembly name</param>
    /// <param name="config">Generator configuration that may override assembly name</param>
    /// <returns>The target namespace to use for generated code</returns>
    string GetTargetNamespace(string assemblyName, IGeneratorConfiguration config);
}