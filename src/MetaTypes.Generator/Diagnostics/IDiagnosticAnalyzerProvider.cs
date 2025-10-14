using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MetaTypes.Generator.Diagnostics;

/// <summary>
/// Provides diagnostic analysis for types discovered by a specific discovery method.
/// Each diagnostic analyzer provider corresponds to one discovery method (matched by Identifier).
/// </summary>
public interface IDiagnosticAnalyzerProvider
{
    /// <summary>
    /// The identifier that matches the discovery method (e.g., "Statics.ServiceMethod").
    /// When this discovery method is enabled in metatypes.config.json, this diagnostic provider will run.
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Description of what this diagnostic analyzer validates.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// All diagnostic descriptors supported by this provider.
    /// These will be registered with the Roslyn diagnostic analyzer.
    /// </summary>
    IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; }

    /// <summary>
    /// Analyzes a type symbol and reports diagnostics if validation rules are violated.
    /// This is called for types that were discovered by the matching discovery method.
    /// </summary>
    /// <param name="context">The symbol analysis context for reporting diagnostics</param>
    /// <param name="typeSymbol">The type symbol to analyze</param>
    /// <param name="relevantAttributes">Attributes relevant to this analyzer (pre-filtered)</param>
    void Analyze(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        ImmutableArray<AttributeData> relevantAttributes);
}
