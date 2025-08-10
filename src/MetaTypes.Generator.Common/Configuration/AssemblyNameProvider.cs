using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Default implementation of namespace handling using MetaTypes' superior approach.
/// This is the exact same logic used by MetaTypes generators - respects real assembly names
/// or uses configured namespace overrides.
/// </summary>
public class AssemblyNameProvider : IAssemblyNameProvider
{
    private static readonly Lazy<AssemblyNameProvider> _instance = new(() => new AssemblyNameProvider());
    
    /// <summary>
    /// Singleton instance for shared usage across generators.
    /// </summary>
    public static AssemblyNameProvider Instance => _instance.Value;

    /// <summary>
    /// Gets the target namespace using MetaTypes' assembly name logic:
    /// - Use explicit configuration if provided (config.GeneratedNamespace)
    /// - Otherwise use the actual assembly name (respecting real assembly names)
    /// 
    /// This follows MetaTypes' approach from MetaTypeSourceGenerator.cs lines 87-88.
    /// </summary>
    public string GetTargetNamespace(Compilation compilation, IGeneratorConfiguration config)
    {
        var assemblyName = compilation.AssemblyName ?? "UnknownAssembly";
        return GetTargetNamespace(assemblyName, config);
    }

    /// <summary>
    /// Gets the target namespace when assembly name is already known.
    /// Uses MetaTypes' logic: explicit namespace config override, or actual assembly name.
    /// </summary>
    public string GetTargetNamespace(string assemblyName, IGeneratorConfiguration config)
    {
        // Use assembly name as namespace, or configured namespace override if provided
        // This is the exact MetaTypes logic from MetaTypeSourceGenerator.cs
        return !string.IsNullOrEmpty(config.GeneratedNamespace) ? config.GeneratedNamespace! : assemblyName;
    }
}