# MetaTypes Diagnostic Infrastructure

## Overview

The MetaTypes generator includes a comprehensive diagnostic infrastructure that validates attribute usage and reports errors/warnings directly in the IDE. Diagnostics are implemented as Roslyn analyzers that run alongside code generation.

## Architecture

### Components

1. **MetaTypesDiagnosticAnalyzer** - Main Roslyn `DiagnosticAnalyzer` that coordinates all diagnostic providers (same level as `MetaTypeSourceGenerator`)
2. **IDiagnosticAnalyzerProvider** - Interface for vendor-specific diagnostic providers
3. **Diagnostic Descriptors** - Static classes defining all diagnostic IDs per vendor
4. **Diagnostic Providers** - Vendor implementations that perform validation logic

### Directory Structure

```
src/MetaTypes.Generator/
├── MetaTypeSourceGenerator.cs                  # Main source generator
├── MetaTypesDiagnosticAnalyzer.cs              # Main diagnostic analyzer (same level)
├── Diagnostics/
│   └── IDiagnosticAnalyzerProvider.cs          # Provider interface
└── Vendor/
    ├── Statics/
    │   └── Diagnostics/
    │       ├── StaticsDiagnostics.cs                       # All Statics diagnostic IDs
    │       └── StaticsServiceMethodDiagnosticProvider.cs   # Validation implementation
    └── EfCore/
        └── Diagnostics/
            ├── EfCoreDiagnostics.cs                        # All EfCore diagnostic IDs
            └── EfCoreDiagnosticProvider.cs                 # Validation implementation (future)
```

## Diagnostic ID Convention

All diagnostic IDs follow this pattern:

```
MT{VendorPrefix}{Number}

Examples:
- MTSTAT0001, MTSTAT0002       # Statics vendor
- MTEFCORE0001, MTEFCORE0002   # EfCore vendor
```

### Vendor Prefixes

| Vendor | Prefix | Range |
|--------|--------|-------|
| Statics | STAT | MTSTAT0001-MTSTAT9999 |
| EfCore | EFCORE | MTEFCORE0001-MTEFCORE9999 |

**Note**: There are no "core" MetaTypes diagnostics. All diagnostics are vendor-specific.

### Number Ranges (per vendor)

| Range | Purpose |
|-------|---------|
| 0001-0099 | Primary feature diagnostics |
| 0100-0199 | Secondary feature diagnostics |
| 0200-0299 | Configuration/setup diagnostics |
| 0300+ | Future use |

## How It Works

### 1. Configuration Integration

Diagnostic providers are automatically enabled based on `metatypes.config.json`:

```json
{
  "DiscoverMethods": [
    "Statics.ServiceMethod"  // ← Enables StaticsServiceMethodDiagnosticProvider
  ]
}
```

When a discovery method is enabled, its corresponding diagnostic provider runs.

### 2. Provider Discovery

Diagnostic providers are **automatically discovered via reflection**, just like vendor generators:

```csharp
// DiagnosticProviderRegistry discovers all implementations
var allProviders = DiagnosticProviderRegistry.GetAllProviders();
```

Providers implement `IDiagnosticAnalyzerProvider`:

```csharp
public class StaticsServiceMethodDiagnosticProvider : IDiagnosticAnalyzerProvider
{
    public string Identifier => "Statics.ServiceMethod";  // Matches discovery method

    public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => new[]
    {
        StaticsDiagnostics.MTSTAT0001_InvalidReturnType,
        StaticsDiagnostics.MTSTAT0002_MissingRouteParameter,
        // ...
    };

    public void Analyze(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        ImmutableArray<AttributeData> relevantAttributes)
    {
        // Validation logic here
    }
}
```

### 3. Diagnostic Reporting

Diagnostics show up in the IDE as squiggly lines and in the Error List:

```csharp
[StaticsServiceMethod(Path = "/users/{id:int}")]
public static string GetUser(string id)  // ← Error: Route param 'id:int' doesn't match parameter type 'string'
{
    return "user";
}
```

### 4. Suppression with Pragmas

Users can suppress diagnostics using standard .NET pragmas:

```csharp
#pragma warning disable MTSTAT0001
[StaticsServiceMethod(Path = "/test")]
public static string InvalidReturnType() => "test";
#pragma warning restore MTSTAT0001
```

Or globally in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.MTSTAT0001.severity = none
```

## Current Diagnostics

### Statics Diagnostics (MTSTAT)

#### ServiceMethod (0001-0099)

| ID | Severity | Description |
|----|----------|-------------|
| MTSTAT0001 | Error | Invalid return type (must be ServiceResult<T>) |
| MTSTAT0002 | Error | Route parameter missing from method signature |
| MTSTAT0003 | Error | Method with 'id' parameter must specify Entity |
| MTSTAT0004 | Error | EntityGlobal with 'id' parameter is invalid |
| MTSTAT0005 | Warning | Entity with 'id' should not be EntityGlobal |
| MTSTAT0006 | Error | Route parameter type mismatch |
| MTSTAT0007 | Error | Path parameter is required |

#### Repository (0100-0199)

| ID | Severity | Description |
|----|----------|-------------|
| MTSTAT0100 | Error | StaticsRepositoryProvider on non-DbContext type |
| MTSTAT0101 | Warning | StaticsRepositoryIgnore on non-DbSet property |

### EfCore Diagnostics (MTEFCORE)

#### DbContext (0001-0099)

| ID | Severity | Description |
|----|----------|-------------|
| MTEFCORE0001 | Error | Type must inherit from DbContext |
| MTEFCORE0002 | Warning | DbContext has no DbSet properties |
| MTEFCORE0003 | Info | Entity missing MetaType attribute |

#### Entity (0100-0199)

| ID | Severity | Description |
|----|----------|-------------|
| MTEFCORE0100 | Warning | Entity missing key property |
| MTEFCORE0101 | Info | Entity has multiple key properties |

## Implementation Guide

### Adding a New Diagnostic

1. **Define the diagnostic** in the vendor's diagnostics file:

```csharp
// In StaticsDiagnostics.cs
public static readonly DiagnosticDescriptor MTSTAT0008_NewDiagnostic = new(
    id: "MTSTAT0008",
    title: "Short title",
    messageFormat: "Detailed message with {0} parameter",
    category: "MetaTypes.Statics",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "Longer description for documentation.");
```

2. **Add to SupportedDiagnostics** in the provider:

```csharp
public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => new[]
{
    // ... existing diagnostics
    StaticsDiagnostics.MTSTAT0008_NewDiagnostic,
};
```

3. **Implement validation logic** in the provider's `Analyze` method:

```csharp
public void Analyze(...)
{
    // Validation logic
    if (someCondition)
    {
        var diagnostic = Diagnostic.Create(
            StaticsDiagnostics.MTSTAT0008_NewDiagnostic,
            location,
            messageArg);
        context.ReportDiagnostic(diagnostic);
    }
}
```

### Adding a New Diagnostic Provider

1. **Create diagnostics file** in `Vendor/{VendorName}/Diagnostics/{VendorName}Diagnostics.cs`
2. **Create provider class** implementing `IDiagnosticAnalyzerProvider`
3. **That's it!** The provider is automatically discovered via reflection

No manual registration needed - providers are discovered automatically just like vendor generators.

## Benefits

1. **Early Feedback** - Errors shown in IDE before build
2. **Consistent** - Same validation in IDE and build
3. **Suppressible** - Standard pragma support
4. **Documented** - All diagnostic IDs in one place per vendor
5. **Separated** - Diagnostic logic separate from generation logic

## Implementation Notes

### Timing: SupportedDiagnostics vs Initialize()

Important: `SupportedDiagnostics` is called **before** `Initialize()` by the Roslyn infrastructure:

1. **SupportedDiagnostics** - Called first to register what diagnostics this analyzer can report
   - Must be fast and deterministic
   - Uses reflection to discover all providers
   - Returns all possible diagnostics from all vendors

2. **Initialize()** - Called second to set up the analyzer
   - Reads `metatypes.config.json` to determine which providers are enabled
   - Only enabled providers actually run their validation logic

This means all diagnostic IDs are registered upfront, but only enabled providers run.

## Future Enhancements

- [ ] Configuration file discovery from AdditionalFiles
- [ ] Custom severity levels via configuration
- [ ] Code fixes for common diagnostics
- [ ] Diagnostic documentation links
- [ ] Performance optimizations for large codebases
