# MetaTypes Project

A unified C# source generator that creates compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime. Features a vendor-based architecture that allows vendors to:

- extend the methods by which types are discovered
- extend the generated MetaTypes by providing own partial class extensions 

## Quick Start

```bash
cd samples/Sample.Console
dotnet run
```

For Entity Framework Core vendor samples:
```bash
cd samples/Vendor/EfCore/Sample.EfCore.SingleProject
dotnet run
```

## Project Structure

### Core Components
- **src/MetaTypes.Abstractions/** - Core interfaces and attributes intended to be used by consumers ([CLAUDE.md](src/MetaTypes.Abstractions/CLAUDE.md))
- **src/MetaTypes.Generator.Common/** - Shared generator infrastructure ([CLAUDE.md](src/MetaTypes.Generator.Common/CLAUDE.md))
- **src/MetaTypes.Generator/** - Main source generator implementation

### Vendor System
- **EfCore Vendor** - Entity Framework Core extensions with table mapping and relationships
- **Statics Vendor** - Static analysis for service method discovery

## Recent Changes (August 2025)

1. **Target-Namespace-Specific DI Extensions** - Generated convenient DI extension methods based on target namespace
2. **Cross-Assembly Mode** - Unified provider generation that consolidates types from multiple assemblies
3. **Vendor-Specific DI Methods** - Separate DI registration methods for EfCore and Statics vendors
4. **Vendor Folder Structure Refactoring** - Refactored MetaTypes.Abstractions to implement the same vendor pattern as other projects
5. **Statics Vendor Implementation** - Added new Statics vendor for service method discovery
6. **Unified Architecture** - Single generator with pluggable vendor system and discovery methods

## Generated DI Extension Methods

MetaTypes now generates convenient DI extension methods based on the target namespace where the generator runs:

### Cross-Assembly Mode
```json
{
  "MetaTypes.Generator": {
    "Discovery": { "CrossAssembly": true }
  }
}
```

Generates one unified registration method:
```csharp
// Register all discovered MetaTypes from multiple assemblies
services.AddMetaTypesSampleConsole();
```

### Single-Project Mode  
```json
{
  "MetaTypes.Generator": {
    "Discovery": { "CrossAssembly": false }
  }
}
```

Generates separate methods per assembly:
```csharp
services.AddMetaTypesSampleBusiness();
services.AddMetaTypesSampleAuth();
```

### Vendor-Specific Methods
```csharp
// EfCore vendor extensions
services.AddMetaTypesSampleConsoleEfCore();

// Statics vendor extensions  
services.AddMetaTypesSampleConsoleStatics();

// Service retrieval
var efCoreTypes = serviceProvider.GetEfCoreMetaTypes();
var staticsTypes = serviceProvider.GetStaticsMetaTypes();
```

## Configuration

Basic project setup with `metatypes.config.json`:
```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    }
  }
}
```

## Documentation

- **[Complete Documentation](./docs/README.md)** - Architecture, configuration, and API reference
- **[Configuration Guide](./CONFIG.md)** - Detailed configuration options
- **[Migration Guide](./MIGRATION.md)** - Upgrading from previous versions

## Build Commands

```bash
dotnet build                    # Build entire solution
dotnet test                     # Run all tests
dotnet build samples/           # Build all samples
```