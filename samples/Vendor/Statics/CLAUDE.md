# Statics Vendor Sample

## Overview
This folder contains samples demonstrating the Statics vendor integration with MetaTypes. The Statics vendor discovers static methods and generates both metadata and repository patterns for unified service access.

## Purpose
The Statics vendor allows you to:
- Discover static methods across your codebase with `[StaticsServiceMethod]` attributes
- Generate compile-time metadata for static service patterns
- Generate repository classes that wrap static methods with consistent async APIs
- Enable dependency injection for both metadata collections and repository implementations
- Organize methods by entity types (User, Order) or as global methods

## Sample Structure
```
Statics/
├── CLAUDE.md                           # This file
└── Sample.Statics.ServiceMethod/       # Complete working sample
    ├── Models/                         # Entity models (User, Order)
    ├── Services/                       # Static service classes
    ├── Program.cs                      # Demo of metadata and repositories
    └── metatypes.config.json          # Configuration
```

## Configuration Example

```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["Statics.StaticMethod"]
    },
    "EnabledVendors": ["Statics"],
    "VendorConfigs": {
      "Statics": {
        "RequireBaseTypes": true,
        "IncludePrivateMethods": false,
        "IncludeInternalMethods": true
      }
    }
  }
}
```

## Key Features

✅ **Static Method Discovery** - Find static methods with `[StaticsServiceMethod]` attributes  
✅ **Service Pattern Support** - Generate metadata for static service patterns  
✅ **Repository Generation** - Generate repository classes with consistent async APIs  
✅ **Entity Classification** - Methods organized by entity types (User, Order) or global  
✅ **Async Consistency** - All repository methods return `Task<>` regardless of original signatures  
✅ **DI Integration** - Service provider extensions for metadata and repositories  
✅ **Cross-Assembly Support** - Works across multiple assemblies  
✅ **Validation** - Comprehensive attribute validation with diagnostics  

## Generated Repository Types

### Entity Repositories
```csharp
public class UserRepository : IStaticsRepository
{
    // Entity-specific methods (with id parameter)
    public Task<ServiceResult<string>> GetUserById(int id) { ... }
    public Task<ServiceResult> UpdateUserStatus(int id, bool isActive, string? reason) { ... }
    
    // Entity-global methods (no id parameter, but Entity = typeof(User))
    public Task<ServiceResult<bool>> ValidateUserData(string username, string email, ...) { ... }
}
```

### Global Repository
```csharp
public class GlobalRepository : IStaticsRepository  
{
    // Global methods (no Entity parameter)
    public Task<ServiceResult<bool>> CreateUser(string userName, string email, bool isActive) { ... }
    public Task<ServiceResult<string>> MigrateUserData(int batchSize) { ... }
}
```

### DI Registration
```csharp
// Register metadata collections
services.AddMetaTypesSampleStaticsServiceMethodStatics();

// Register repository implementations  
services.AddMetaTypesSampleStaticsServiceMethodStaticsRepositories();

// Retrieve repositories
var repositories = serviceProvider.GetServices<IStaticsRepository>();
var userRepo = serviceProvider.GetService<UserRepository>();
var globalRepo = serviceProvider.GetService<GlobalRepository>();
```

## Method Classification

Methods are automatically classified based on their `[StaticsServiceMethod]` attribute:

1. **Entity-Specific**: `Entity = typeof(User)` + has `id` parameter → Goes in `UserRepository`
2. **Entity-Global**: `Entity = typeof(User), EntityGlobal = true` + no `id` parameter → Goes in `UserRepository`  
3. **Global**: No `Entity` parameter → Goes in `GlobalRepository`

## Async Method Handling

The repository generator handles both sync and async service methods:

```csharp
// Original: ServiceResult<string> GetUserById(int id)
// Generated: Task<ServiceResult<string>> GetUserById(int id) => Task.FromResult(original(...))

// Original: Task<ServiceResult<string>> GetUserAsync(int id)  
// Generated: Task<ServiceResult<string>> GetUserAsync(int id) => original(...) 
```

## Implementation Details

The Statics vendor is implemented in:
- **Discovery**: `src/MetaTypes.Generator.Common/Vendor/Statics/Discovery/`
- **Generation**: `src/MetaTypes.Generator.Common/Vendor/Statics/Generation/`
- **Abstractions**: `src/MetaTypes.Abstractions/Vendor/Statics/`

## Status
✅ **Complete** - Full repository generation feature with working sample.