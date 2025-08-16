# Sample.Statics.ServiceMethod

## Overview
Complete working sample demonstrating the Statics vendor with repository generation. Shows how static service methods are discovered and wrapped in repository classes with consistent async APIs.

## Features Demonstrated

### 1. Static Service Method Discovery
- Methods marked with `[StaticsServiceMethod]` attribute
- Entity binding with `Entity = typeof(User)` or `Entity = typeof(Order)`
- Entity-global vs entity-specific method classification
- Global methods without entity binding

### 2. Repository Generation
- **UserRepository**: Entity-specific and entity-global User methods
- **OrderRepository**: Entity-specific Order methods  
- **GlobalRepository**: Global methods without entity binding
- All repository methods return `Task<>` for consistency

### 3. Async Method Support
- Original sync methods: `ServiceResult<T>` → `Task<ServiceResult<T>>` (wrapped)
- Original async methods: `Task<ServiceResult<T>>` → `Task<ServiceResult<T>>` (passed through)

### 4. DI Integration
- Metadata registration: `AddMetaTypesSampleStaticsServiceMethodStatics()`
- Repository registration: `AddMetaTypesSampleStaticsServiceMethodStaticsRepositories()`
- Interface-based retrieval: `GetServices<IStaticsRepository>()`

## Project Structure

```
Sample.Statics.ServiceMethod/
├── Models/
│   ├── User.cs                    # Entity with [MetaType] attribute
│   └── Order.cs                   # Entity with [MetaType] attribute
├── Services/
│   ├── UserServices.cs            # Static methods with [StaticsServiceMethod]
│   └── OrderServices.cs           # Static methods with [StaticsServiceMethod]
├── Program.cs                     # Demo application
└── metatypes.config.json         # Generator configuration
```

## Generated Files

When you build this project, the generator creates:

### Metadata Files
- `Sample.Statics.ServiceMethod_UserServicesMetaTypeStatics.g.cs`
- `Sample.Statics.ServiceMethod_OrderServicesMetaTypeStatics.g.cs`
- `StaticsServiceCollectionExtensions.g.cs`

### Repository Files
- `UserRepository.g.cs` - Wraps User entity methods
- `OrderRepository.g.cs` - Wraps Order entity methods  
- `GlobalRepository.g.cs` - Wraps global methods
- `StaticsRepositoryServiceCollectionExtensions.g.cs` - DI registration

### Diagnostic Files
- `_StaticsValidationDiagnostic.g.cs` - Validation errors and warnings

## Method Examples

### Entity-Specific Methods
```csharp
[StaticsServiceMethod(Path = "/users/{id:int}", Entity = typeof(User))]
public static ServiceResult<string> GetUserById(int id) => // ...

// Generated repository method:
public Task<ServiceResult<string>> GetUserById(int id)
{
    var result = UserServices.GetUserById(id);
    return Task.FromResult(result);
}
```

### Entity-Global Methods  
```csharp
[StaticsServiceMethod(Path = "/users", EntityGlobal = true)]
public static ServiceResult<bool> CreateUser(string userName, string email) => // ...

// Generated in GlobalRepository (because no Entity specified):
public Task<ServiceResult<bool>> CreateUser(string userName, string email, bool isActive)
{
    var result = UserServices.CreateUser(userName, email, isActive);
    return Task.FromResult(result);
}
```

### Async Methods
```csharp
[StaticsServiceMethod(Path = "/users/{id:int}/detailed", Entity = typeof(User))]  
public static async Task<ServiceResult<string>> GetUserWithLoggingAsync(int id, ILogger logger, ...) => // ...

// Generated repository method (passed through):
public Task<ServiceResult<string>> GetUserWithLoggingAsync(int id, ILogger logger, ...)
{
    return UserServices.GetUserWithLoggingAsync(id, logger, ...);
}
```

## Running the Sample

```bash
cd samples/Vendor/Statics/Sample.Statics.ServiceMethod
dotnet run
```

Expected output:
```
=== Statics ServiceMethod Enhanced DI Demo ===
✅ Retrieved 2 Statics MetaTypes via vendor-specific DI:
  - OrderServices: Service Methods: 3
  - UserServices: Service Methods: 7

=== Testing Generated Repositories ===
✅ Retrieved 3 Statics Repositories via DI:
  - OrderRepository
  - UserRepository  
  - GlobalRepository

✅ Testing UserRepository methods:
  GetUserById(123): User 123
  GetUserWithLoggingAsync(123): User 123 (logged)

✅ Testing GlobalRepository methods:
  CreateUser: Success (True)
  MigrateUserData: Migrated 100 users
```

## Key Concepts

### Repository Classification
- **Entity Repositories**: Methods with `Entity = typeof(T)` → `{EntityName}Repository`
- **Global Repository**: Methods without `Entity` → `GlobalRepository`
- **Async Consistency**: All repository methods return `Task<>` regardless of original signature

### DI Integration  
- Each repository implements `IStaticsRepository` 
- Entity repositories with EfCore backing also implement `IEntityRepository`
- Individual repositories available by concrete type

### Validation
- Return type validation: Must return `ServiceResult<T>` or `ServiceResult`
- Route parameter validation: `{id:int}` must match method parameter types
- Entity binding validation: Methods with `id` parameter must specify `Entity`

This sample demonstrates the complete repository generation feature, showing how static service methods can be unified into a consistent repository pattern with strong typing and async support.