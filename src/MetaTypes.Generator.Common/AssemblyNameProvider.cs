using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Standard implementation of assembly name detection logic used by MetaTypes.
/// This implementation uses the proven logic from MetaTypes for consistent
/// assembly name handling across all source generators.
/// </summary>
public class AssemblyNameProvider : IAssemblyNameProvider
{
    /// <summary>
    /// Singleton instance for shared usage across generators.
    /// </summary>
    public static AssemblyNameProvider Instance { get; } = new();

    /// <inheritdoc />
    public string GetTargetNamespace(Compilation compilation, IGeneratorConfiguration config)
    {
        var assemblyName = GetAssemblyName(compilation);
        return GetTargetNamespace(assemblyName, config);
    }

    /// <inheritdoc />
    public string GetAssemblyName(Compilation compilation)
    {
        return compilation.AssemblyName ?? "UnknownAssembly";
    }

    /// <inheritdoc />
    public string GetTargetNamespace(string assemblyName, IGeneratorConfiguration config)
    {
        // Use configured override if provided, otherwise use assembly name directly
        // This follows MetaTypes' proven logic: prefer explicit configuration,
        // fallback to actual assembly name without aggressive suffix removal
        return !string.IsNullOrEmpty(config.AssemblyName) ? config.AssemblyName! : assemblyName;
    }
}