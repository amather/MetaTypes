# MetaTypes.Generator.Common

## Overview
This folder contains shared source generator code that gets linked into the main MetaTypes.Generator project. It provides the core infrastructure for the unified vendor-based generator architecture.

## Key Components

### Generator/ - Core Generator Classes
- **CoreMetaTypeGenerator** - Static class providing MetaType class generation and DI extension method generation
- **NamingUtils** - Shared utilities for consistent naming across generators (ToPascalCase, ToMethodName, ToAddMetaTypesMethodName)
- **IVendorGenerator** - Interface for vendor-specific generators
- **VendorGeneratorRegistry** - Registry for discovering and managing vendor generators

### Discovery/ - Pluggable Type Discovery System
- **DiscoveredType** - Model representing a discovered type with discovery metadata and helper methods
- **UnifiedTypeDiscovery** - Reflection-based discovery coordinator that finds and executes IDiscoveryMethod implementations
- **IDiscoveryMethod** - Interface for pluggable discovery methods with unique identifiers
- **AttributesDiscoveryMethod** - Core plugin for discovering types with [MetaType] attribute (`"MetaTypes.Attribute"`)
- **ReferencesDiscoveryMethod** - Core plugin for discovering referenced types with [MetaType] (`"MetaTypes.Reference"`)

### Configuration/ - Unified Configuration System
- **MetaTypesGeneratorConfiguration** - Root configuration parser for `"MetaTypes.Generator"` section
- **BaseGeneratorOptions** - Main configuration options with Generation, Discovery, and Vendors sections
- **ConfigurationLoader** - Helper class for loading and parsing metatypes.config.json
- **DiscoveryConfig** - Configuration for type discovery (Syntax, CrossAssembly, Methods array)
- **DiscoveryMethodsConfig** - Array-based discovery methods configuration (e.g., `["MetaTypes.Attribute", "EfCore.TableAttribute"]`)
- **GenerationConfig** - Generation options (BaseMetaTypes flag)
- **VendorConfig** - Vendor-specific configuration (EfCore options)
- **AssemblyNameProvider** - Implementation of IAssemblyNameProvider for providing assembly names

## Usage
This code is linked into the main MetaTypes.Generator project via:
```xml
<Compile Include="../MetaTypes.Generator.Common/**/*.cs" LinkBase="Common" />
```

## Architecture
The unified generator architecture works as follows:

### Discovery Process
1. **Configuration Loading**: `ConfigurationLoader` parses `metatypes.config.json`
2. **Method Resolution**: `UnifiedTypeDiscovery` uses reflection to find all `IDiscoveryMethod` implementations
3. **Type Discovery**: Each enabled discovery method finds types and creates `DiscoveredType` instances
4. **Aggregation**: Multiple discovery methods can find the same type, results are aggregated

### Generation Process
1. **Base Generation**: Standard MetaType classes and DI extensions are generated (if `BaseMetaTypes: true`)
2. **Vendor Generation**: Vendor generators extend base types with additional interfaces (e.g., `IMetaTypeEfCore`)
3. **DI Extension Generation**: Both core and vendor-specific DI extension methods are generated
4. **Coordination**: Vendor generators filter types using `DiscoveredType.WasDiscoveredByPrefix("EfCore.")`

### DI Extension Method Generation
The generator now produces target-namespace-specific DI extension methods:

#### Cross-Assembly Mode (`CrossAssembly: true`)
- Generates one unified provider class in the target namespace (where generator runs)
- Creates one DI method: `AddMetaTypes{TargetNamespace}()`
- Example: `AddMetaTypesSampleConsole()` registers all types from Sample.Auth and Sample.Business

#### Single-Project Mode (`CrossAssembly: false`)  
- Generates separate provider classes per source assembly
- Creates separate DI methods per assembly: `AddMetaTypes{AssemblyName}()`
- Example: `AddMetaTypesSampleBusiness()` and `AddMetaTypesSampleAuth()`

#### Vendor-Specific Extensions
- Each vendor generates its own DI extension class
- Method format: `AddMetaTypes{TargetNamespace}{VendorName}()`
- Service retrieval: `Get{VendorName}MetaTypes()` methods for individual entities
- DbContext collections: `GetEfCoreDbContexts()`, `GetEfCoreDbContext<T>()` methods
- Example: `AddMetaTypesSampleConsoleEfCore()` and `serviceProvider.GetEfCoreMetaTypes()`

## Directory Structure
```
MetaTypes.Generator.Common/
├── Configuration/           # Configuration management classes
│   ├── MetaTypesGeneratorConfiguration.cs
│   ├── DiscoveryConfig.cs
│   ├── DiscoveryMethodsConfig.cs
│   ├── GenerationConfig.cs
│   ├── GeneratorConfigSection.cs
│   ├── BaseGeneratorOptions.cs
│   ├── EfCoreGeneratorOptions.cs
│   ├── EfCoreSpecificConfig.cs
│   ├── EfCoreDiscoveryMethodsConfig.cs
│   ├── ConfigurationLoader.cs
│   └── AssemblyNameProvider.cs
├── Discovery/               # Type discovery system
│   ├── DiscoveredType.cs
│   ├── UnifiedTypeDiscovery.cs
│   ├── IDiscoveryMethod.cs
│   ├── AttributesDiscoveryMethod.cs
│   └── ReferencesDiscoveryMethod.cs
├── Generator/               # Core generator classes
│   └── CoreMetaTypeGenerator.cs
└── Vendor/                  # Vendor-specific extensions
    └── EfCore/             # Entity Framework Core vendor
        ├── Generation/     # EfCore vendor generator
        │   └── EfCoreVendorGenerator.cs
        └── Discovery/      # EfCore discovery methods
            ├── EfCoreEntitiesDiscoveryMethod.cs    # "EfCore.TableAttribute"
            └── DbContextScanningDiscoveryMethod.cs # "EfCore.DbContextSet"
```

## Vendor System

### New Vendor Architecture (August 2025)

**Vendor-Agnostic Core**: The generator has no hard-coded knowledge of specific vendors. All vendor logic is pluggable.

**Fixed Critical Bug**: Vendor generation now runs independently of base generation - vendors are no longer incorrectly skipped when `BaseMetaTypes = false`.

**Explicit Configuration:**
```json
{
  "EnabledVendors": ["EfCore"],
  "VendorConfigs": {
    "EfCore": {
      "RequireBaseTypes": true,
      "IncludeNavigationProperties": true,
      "IncludeForeignKeys": true
    }
  }
}
```

**Self-Configuring Vendors**: Each vendor implements `Configure(JsonElement? config)` and parses its own configuration:
```csharp
public void Configure(JsonElement? config)
{
    if (config.HasValue)
    {
        _config = JsonSerializer.Deserialize<EfCoreConfig>(config.Value) ?? new EfCoreConfig();
    }
}
```

### Available Vendors
- **EfCore Vendor**: Generates Entity Framework Core extensions with table names, keys, relationships, and DbContext collections
- **Statics Vendor**: Generates static service method metadata and repository patterns with consistent async APIs

### Discovery Methods by Vendor
- **Core Methods**:
  - `"MetaTypes.Attribute"` - Types with `[MetaType]` attribute
  - `"MetaTypes.Reference"` - Referenced types with `[MetaType]`
- **EfCore Methods**:
  - `"EfCore.TableAttribute"` - Types with `[Table]` attribute (syntax-based)
  - `"EfCore.DbContextSet"` - Entity types found via `DbSet<T>` properties
- **Statics Methods**:
  - `"Statics.Attribute"` - Static classes with `[StaticsServiceMethod]` attributed methods

### Generated Files Structure

#### EfCore Vendor Files
For a type discovered by EfCore methods:
- **Base File**: `TestEntityMetaType.g.cs` (implements `IMetaType<TestEntity>`) - requires `BaseMetaTypes: true`
- **EfCore Extension**: `TestEntityEfCoreMetaType.g.cs` (partial class implementing `IMetaTypeEfCore`)
- **DbContext Collections**: `{ContextName}MetaTypesEfCoreDbContext.g.cs` (implements `IMetaTypesEfCoreDbContext`)

#### Statics Vendor Files
For a static class discovered by Statics methods:
- **Base File**: `TestServicesMetaType.g.cs` (implements `IMetaType<TestServices>`) - requires `BaseMetaTypes: true`
- **Statics Extension**: `TestServicesMetaTypeStatics.g.cs` (partial class implementing `IMetaTypeStatics`)
- **Repository Files**: `{EntityName}Repository.g.cs` and `GlobalRepository.g.cs` (implement `IStaticsRepository`)
- **DI Extensions**: `StaticsServiceCollectionExtensions.g.cs` and `StaticsRepositoryServiceCollectionExtensions.g.cs`

### EfCore DbContext Collection Features

The EfCore vendor now generates DbContext collections that organize entity types by their originating DbContext:

#### Generated DbContext Classes
```csharp
public class CustomerDbContextMetaTypesEfCoreDbContext : IMetaTypesEfCoreDbContext
{
    public string ContextName => "CustomerDbContext";
    public Type ContextType => typeof(CustomerDbContext);
    public IEnumerable<IMetaTypeEfCore> EntityTypes => /* filtered entities */;
}
```

#### Entity Grouping Strategy
- **DbContext Scanning Discovery** (`EfCore.DbContextSet`): Groups entities by their actual DbContext class name
- **Table Attribute Discovery** (`EfCore.TableAttribute`): Groups entities in "UnknownContext" since they could belong to multiple DbContexts
- **Enhanced Discovery Context**: Stores `DbContextName` and `DbContextType` in `DiscoveredType.DiscoveryContexts`

#### DI Registration
```csharp
// Enhanced service registration
services.AddSingleton<IMetaTypesEfCoreDbContext>(new CustomerDbContextMetaTypesEfCoreDbContext());
services.AddSingleton<IMetaTypesEfCoreDbContext>(new UnknownContextMetaTypesEfCoreDbContext());
```

#### Service Provider Extensions
```csharp
// Get all DbContext collections
public static IEnumerable<IMetaTypesEfCoreDbContext> GetEfCoreDbContexts(this IServiceProvider)

// Get specific DbContext metadata
public static IMetaTypesEfCoreDbContext? GetEfCoreDbContext<TDbContext>(this IServiceProvider)
public static IMetaTypesEfCoreDbContext? GetEfCoreDbContext(this IServiceProvider, Type dbContextType)
```

### Statics Repository Generation Features

The Statics vendor generates repository classes that wrap static service methods with consistent async APIs:

#### Generated Repository Classes
```csharp
// Entity Repository (for methods with Entity = typeof(User))
public class UserRepository : IStaticsRepository
{
    // Entity-specific methods (with id parameter)
    public Task<ServiceResult<string>> GetUserById(int id) => Task.FromResult(UserServices.GetUserById(id));
    
    // Entity-global methods (no id parameter, but Entity = typeof(User))
    public Task<ServiceResult<bool>> ValidateUserData(string username, string email, ...) => /* ... */;
}

// Global Repository (for methods without Entity)
public class GlobalRepository : IStaticsRepository  
{
    public Task<ServiceResult<bool>> CreateUser(string userName, string email, bool isActive) => /* ... */;
}
```

#### Repository Method Classification
- **Entity-Specific**: `Entity = typeof(User)` + has `id` parameter → Goes in `UserRepository`
- **Entity-Global**: `Entity = typeof(User), EntityGlobal = true` + no `id` parameter → Goes in `UserRepository`  
- **Global**: No `Entity` parameter → Goes in `GlobalRepository`

#### Async Consistency
All repository methods return `Task<>` regardless of original method signatures:
- Original sync methods: `ServiceResult<T>` → wrapped with `Task.FromResult()`
- Original async methods: `Task<ServiceResult<T>>` → passed through directly

#### DI Registration
```csharp
// Register repositories
services.AddMetaTypesSampleStaticsServiceMethodStaticsRepositories();

// Retrieve repositories
var repositories = serviceProvider.GetServices<IStaticsRepository>();
var userRepo = serviceProvider.GetService<UserRepository>();
var globalRepo = serviceProvider.GetService<GlobalRepository>();
```

### Vendor Dependencies
- **EfCore Vendor**: Requires base MetaTypes by default (`RequireBaseTypes: true`) since extensions are partial classes
- **Statics Vendor**: Requires base MetaTypes by default (`RequireBaseTypes: true`) since extensions are partial classes
- **Independent Generation**: Vendor generation runs separately from base generation but may depend on base types existing
- **DbContext Collection Support**: Works with both `CrossAssembly: true/false` configurations
- **Repository Generation**: Works with both `CrossAssembly: true/false` configurations