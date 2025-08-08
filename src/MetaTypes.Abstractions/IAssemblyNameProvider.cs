using Microsoft.CodeAnalysis;

namespace MetaTypes.Abstractions;

/// <summary>
/// Interface for providing consistent assembly name handling across MetaTypes generators.
/// Implements the superior MetaTypes approach of respecting real assembly names
/// instead of aggressive suffix removal.
/// </summary>
public interface IAssemblyNameProvider
{
    /// <summary>
    /// Gets the target namespace for generated code, using MetaTypes' assembly name logic:
    /// - Use explicit configuration if provided
    /// - Otherwise use the actual assembly name (respecting real assembly names)
    /// </summary>
    string GetTargetNamespace(Compilation compilation, IGeneratorConfiguration config);
    
    /// <summary>
    /// Gets the target namespace for generated code when assembly name is already known.
    /// </summary>
    string GetTargetNamespace(string assemblyName, IGeneratorConfiguration config);
}