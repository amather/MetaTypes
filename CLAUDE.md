# MetaTypes Project

A C# source generator that produces compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime. Implements a vendor-based architecture that allows extensibility through:

- Custom type discovery methods
- Vendor-specific metadata extensions via partial class generation

## Quick Start

Primary showcase (Entity Framework Core vendor):
```bash
cd samples/Vendor/EfCore/Sample.EfCore.SingleProject
dotnet run
```

Basic functionality:
```bash
cd samples/Sample.Console
dotnet run
```

## Project Structure

### Core Components
- **src/MetaTypes.Abstractions/** - Core interfaces and attributes intended to be used by consumers ([CLAUDE.md](src/MetaTypes.Abstractions/CLAUDE.md))
- **src/MetaTypes.Generator.Common/** - Shared generator infrastructure ([CLAUDE.md](src/MetaTypes.Generator.Common/CLAUDE.md))
- **src/MetaTypes.Generator/** - Main source generator implementation

### Vendor System
- **EfCore Vendor** - Entity Framework Core extensions with table mapping and relationships
- **Statics Vendor** - Service method discovery for static classes

## Recent Architecture Changes

1. **Target-Namespace-Specific DI Extensions** - Generate DI extension methods based on target namespace
2. **Cross-Assembly Mode** - Unified provider generation that consolidates types from multiple assemblies
3. **Vendor-Specific DI Methods** - Separate DI registration methods for each vendor
4. **Vendor Folder Structure** - Consistent vendor pattern across all projects
5. **Pluggable Vendor System** - Vendor-agnostic core with specialized extensions

## Generated DI Extension Methods

The generator produces DI extension methods based on the target namespace where the generator runs:

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