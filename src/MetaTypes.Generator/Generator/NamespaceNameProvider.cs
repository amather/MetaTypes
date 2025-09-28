using Microsoft.CodeAnalysis;
using MetaTypes.Abstractions;
using MetaTypes.Generator.Configuration;

namespace MetaTypes.Generator.Generator;

/// <summary>
/// Helper class to provide consistent assembly names
/// </summary>
public static class NamespaceNameProvider 
{
    /// <summary>
    /// Gets the target namespace: 
    /// - Use explicit configuration if provided (config.GeneratedNamespace)
    /// - Otherwise use the actual assembly name (respecting real assembly names)
    /// </summary>
    public static string GetTargetNamespace(Compilation compilation, MetaTypesOptions config)
    {
        var assemblyName = compilation.AssemblyName ?? "UnknownAssembly";
        return !string.IsNullOrEmpty(config.GeneratedNamespace) ? config.GeneratedNamespace! : assemblyName;
    }

    /// <summary>
    /// Gets the target namespace: 
    /// - Use explicit configuration if provided (config.GeneratedNamespace)
    /// - Otherwise use the actual assembly name (respecting real assembly names)
    /// </summary>
    public static string GetTargetNamespace(string assemblyName, MetaTypesOptions config)
    {
        return !string.IsNullOrEmpty(config.GeneratedNamespace) ? config.GeneratedNamespace! : assemblyName;
    }
}