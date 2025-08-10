using Microsoft.CodeAnalysis;

namespace MetaTypes.Generator.Common;

/// <summary>
/// Interface for pluggable discovery methods that can find types for MetaType generation.
/// Implementations can be registered and configured via string identifiers.
/// </summary>
public interface IDiscoveryMethod
{
    /// <summary>
    /// Unique identifier for this discovery method used in configuration.
    /// Should be lowercase with hyphens (e.g., "attributes", "efcore-entities").
    /// </summary>
    string Identifier { get; }
    
    /// <summary>
    /// Human-readable description of what this discovery method does.
    /// Used for documentation and error messages.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Indicates whether this method requires cross-assembly compilation.
    /// Used to optimize discovery when only syntax-based discovery is needed.
    /// </summary>
    bool RequiresCrossAssembly { get; }
    
    /// <summary>
    /// Discovers types using this method's specific logic.
    /// </summary>
    /// <param name="compilation">The compilation context with syntax trees and references.</param>
    /// <returns>Collection of discovered types with discovery metadata.</returns>
    IEnumerable<DiscoveredType> Discover(Compilation compilation);
    
    /// <summary>
    /// Validates whether this discovery method can run in the current context.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>True if the method can run, false if it should be skipped.</returns>
    bool CanRun(Compilation compilation);
}