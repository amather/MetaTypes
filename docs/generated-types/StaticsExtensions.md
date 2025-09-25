# Statics Extensions

When the Statics vendor is enabled, MetaTypes automatically generates additional partial class extensions that implement Statics-specific interfaces. These extensions provide rich metadata about static service methods, including parameter details, attributes, and repository patterns with consistent async APIs.

## Overview

The Statics extensions enhance MetaType classes with static service method metadata and generate repository classes that wrap static methods:

- **IMetaTypeStatics**: Adds service method collections to MetaType classes
- **IStaticsServiceMethod**: Provides detailed metadata about individual static methods
- **Repository Classes**: Generate async-consistent wrappers around static service methods
- **IStaticsRepository**: Marker interface for all generated repositories

## Auto-Detection and Configuration

Statics extensions are enabled when:
1. The "Statics" vendor is included in `EnabledVendors` configuration
2. At least one type is discovered through Statics discovery methods
3. Static classes contain methods with `[StaticsServiceMethod]` attribute

**Configuration:**
```json
{
  "MetaTypes.Generator": {
    "EnabledVendors": ["Statics"],
    "VendorConfigs": {
      "Statics": {
        "RequireBaseTypes": true,
        "GenerateRepositories": true
      }
    },
    "Discovery": {
      "Methods": ["Statics.Attribute"]
    }
  }
}
```

## IMetaTypeStatics Interface

### Interface Definition

```csharp
public interface IMetaTypeStatics
{
    IReadOnlyList<IStaticsServiceMethod> ServiceMethods { get; }
}
```

### Generated Implementation

```csharp
public partial class UserServicesMetaType : IMetaType, IMetaType<UserServices>, IMetaTypeStatics
{
    // Statics-specific properties
    public IReadOnlyList<IStaticsServiceMethod> ServiceMethods => [
        UserServicesStaticsServiceMethodGetUserById.Instance,
        UserServicesStaticsServiceMethodCreateUser.Instance,
        UserServicesStaticsServiceMethodValidateUserData.Instance,
        // ... other service methods
    ];
}
```

## IStaticsServiceMethod Interface

### Interface Definition

```csharp
public interface IStaticsServiceMethod
{
    string MethodName { get; }
    Type ReturnType { get; }
    IReadOnlyList<IStaticsAttributeInfo> MethodAttributes { get; }
    IReadOnlyList<IStaticsParameterInfo> Parameters { get; }
}
```

### Generated Implementation

```csharp
public partial class UserServicesStaticsServiceMethodGetUserById : IStaticsServiceMethod
{
    private static UserServicesStaticsServiceMethodGetUserById? _instance;
    public static UserServicesStaticsServiceMethodGetUserById Instance => _instance ??= new();

    public string MethodName => "GetUserById";
    public Type ReturnType => typeof(ServiceResult<User>);

    public IReadOnlyList<IStaticsAttributeInfo> MethodAttributes => [
        // Generated attribute metadata
    ];

    public IReadOnlyList<IStaticsParameterInfo> Parameters => [
        UserServicesStaticsServiceMethodGetUserByIdParameterId.Instance,
        // ... other parameters
    ];
}
```

## Service Method Attributes

### StaticsServiceMethod Attribute Usage

```csharp
public static class UserServices
{
    [StaticsServiceMethod(Entity = typeof(User))]
    public static ServiceResult<User> GetUserById(int id)
    {
        // Implementation
    }

    [StaticsServiceMethod(Entity = typeof(User), EntityGlobal = true)]
    public static async Task<ServiceResult<bool>> ValidateUserData(string username, string email)
    {
        // Implementation
    }

    [StaticsServiceMethod] // Global method (no Entity)
    public static ServiceResult<bool> CreateUser(string userName, string email, bool isActive)
    {
        // Implementation
    }
}
```

**Attribute Properties:**
- `Entity`: Specifies the entity type this method operates on
- `EntityGlobal`: Indicates method operates on entity but doesn't require specific instance

## Repository Generation

The Statics vendor generates repository classes that provide consistent async APIs around static service methods:

### Repository Classification

**Entity-Specific Repositories:**
- Methods with `Entity = typeof(User)` and `id` parameter → `UserRepository`
- Methods with `Entity = typeof(User), EntityGlobal = true` → `UserRepository`

**Global Repository:**
- Methods without `Entity` parameter → `GlobalRepository`

### Generated Repository Example

```csharp
public class UserRepository : IStaticsRepository
{
    private static UserRepository? _instance;
    public static UserRepository Instance => _instance ??= new();

    // Entity-specific methods (with id parameter)
    public Task<ServiceResult<User>> GetUserById(int id)
    {
        return Task.FromResult(UserServices.GetUserById(id));
    }

    // Entity-global methods (no id parameter, but Entity = typeof(User))
    public Task<ServiceResult<bool>> ValidateUserData(string username, string email)
    {
        // Original method is async, pass through directly
        return UserServices.ValidateUserData(username, email);
    }
}

public class GlobalRepository : IStaticsRepository
{
    private static GlobalRepository? _instance;
    public static GlobalRepository Instance => _instance ??= new();

    // Global methods (no Entity parameter)
    public Task<ServiceResult<bool>> CreateUser(string userName, string email, bool isActive)
    {
        return Task.FromResult(UserServices.CreateUser(userName, email, isActive));
    }
}
```

### Async Consistency

All repository methods return `Task<>` regardless of original method signatures:
- **Synchronous methods**: Wrapped with `Task.FromResult()`
- **Asynchronous methods**: Passed through directly
- **Consistent API**: All callers can use `await` uniformly

## Parameter and Attribute Metadata

### IStaticsParameterInfo Interface

```csharp
public interface IStaticsParameterInfo
{
    string ParameterName { get; }
    Type ParameterType { get; }
    IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes { get; }
}
```

### IStaticsAttributeInfo Interface

```csharp
public interface IStaticsAttributeInfo
{
    Type AttributeType { get; }
    IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments { get; }
    IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments { get; }
}
```

### Generated Attribute Metadata

```csharp
public partial class UserServicesStaticsServiceMethodGetUserByIdParameterId : IStaticsParameterInfo
{
    public string ParameterName => "id";
    public Type ParameterType => typeof(int);

    public IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes => [
        // Generated attribute instances for parameters like [Range], [Required], etc.
    ];
}
```

## Dependency Injection Integration

### Service Registration

```csharp
// Register Statics metadata
services.AddMetaTypesSampleConsoleStatics();

// Register repositories
services.AddMetaTypesSampleConsoleStaticsRepositories();
```

### Generated DI Extensions

```csharp
public static class StaticsServiceCollectionExtensions
{
    public static IServiceCollection AddMetaTypesSampleConsoleStatics(this IServiceCollection services)
    {
        // Register Statics metadata providers
        return services;
    }
}

public static class StaticsRepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddMetaTypesSampleConsoleStaticsRepositories(this IServiceCollection services)
    {
        // Register repository instances
        services.AddSingleton<IStaticsRepository, UserRepository>();
        services.AddSingleton<IStaticsRepository, GlobalRepository>();
        services.AddSingleton<UserRepository>(UserRepository.Instance);
        services.AddSingleton<GlobalRepository>(GlobalRepository.Instance);
        return services;
    }
}
```

### Service Retrieval

```csharp
// Get all repositories
var repositories = serviceProvider.GetServices<IStaticsRepository>();

// Get specific repositories
var userRepository = serviceProvider.GetService<UserRepository>();
var globalRepository = serviceProvider.GetService<GlobalRepository>();

// Get Statics metadata
var userStaticsMetaType = serviceProvider.GetService<IMetaType<UserServices>>();
if (userStaticsMetaType is IMetaTypeStatics staticsType)
{
    foreach (var method in staticsType.ServiceMethods)
    {
        Console.WriteLine($"Method: {method.MethodName}");
        Console.WriteLine($"Returns: {method.ReturnType.Name}");
    }
}
```

## Usage Patterns

### Service Method Analysis

```csharp
var userServicesMetaType = serviceProvider.GetService<IMetaType<UserServices>>();

if (userServicesMetaType is IMetaTypeStatics staticsType)
{
    Console.WriteLine($"Service Class: {userServicesMetaType.ManagedTypeName}");
    Console.WriteLine($"Service Methods: {staticsType.ServiceMethods.Count}");

    foreach (var method in staticsType.ServiceMethods)
    {
        Console.WriteLine($"\nMethod: {method.MethodName}");
        Console.WriteLine($"  Returns: {method.ReturnType.Name}");
        Console.WriteLine($"  Parameters: {method.Parameters.Count}");

        foreach (var param in method.Parameters)
        {
            Console.WriteLine($"    {param.ParameterName}: {param.ParameterType.Name}");
        }

        if (method.MethodAttributes.Any())
        {
            Console.WriteLine($"  Attributes: {method.MethodAttributes.Count}");
        }
    }
}
```

### Repository Usage

```csharp
public class UserController
{
    private readonly UserRepository _userRepository;
    private readonly GlobalRepository _globalRepository;

    public UserController(UserRepository userRepository, GlobalRepository globalRepository)
    {
        _userRepository = userRepository;
        _globalRepository = globalRepository;
    }

    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userRepository.GetUserById(id);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        // Validate first using entity-global method
        var isValid = await _userRepository.ValidateUserData(request.Username, request.Email);
        if (!isValid.Data)
        {
            return BadRequest("Invalid user data");
        }

        // Create using global method
        var result = await _globalRepository.CreateUser(request.Username, request.Email, true);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }
}
```

### Cross-Assembly Repository Discovery

```csharp
public class RepositoryAnalyzer
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryAnalyzer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AnalyzeRepositories()
    {
        var repositories = _serviceProvider.GetServices<IStaticsRepository>();

        Console.WriteLine($"Total Repositories: {repositories.Count()}");

        foreach (var repository in repositories)
        {
            var repositoryType = repository.GetType();
            Console.WriteLine($"\nRepository: {repositoryType.Name}");

            // Analyze repository methods via reflection
            var methods = repositoryType.GetMethods()
                .Where(m => m.IsPublic && !m.IsStatic && m.DeclaringType == repositoryType);

            foreach (var method in methods)
            {
                var isAsync = method.ReturnType.IsGenericType &&
                             method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                Console.WriteLine($"  {method.Name}: {method.ReturnType.Name} (Async: {isAsync})");
            }
        }
    }
}
```

## Discovery Methods

### Statics.Attribute Discovery

The `Statics.Attribute` discovery method finds static classes containing methods with `[StaticsServiceMethod]` attributes:

```csharp
[MetaType]
public static class UserServices
{
    [StaticsServiceMethod(Entity = typeof(User))]
    public static ServiceResult<User> GetUserById(int id) { /* ... */ }

    // Regular static methods without attribute are ignored
    public static void InternalHelper() { /* ... */ }
}
```

**Discovery Criteria:**
- Class must be `static`
- Class must be marked with `[MetaType]` attribute
- Class must contain at least one method with `[StaticsServiceMethod]`
- Only attributed methods are included in metadata

## Configuration Examples

### Basic Statics Configuration

```json
{
  "MetaTypes.Generator": {
    "EnabledVendors": ["Statics"],
    "Discovery": {
      "Methods": ["Statics.Attribute"]
    }
  }
}
```

### Full Statics Configuration

```json
{
  "MetaTypes.Generator": {
    "Generation": {
      "BaseMetaTypes": true
    },
    "EnabledVendors": ["Statics", "EfCore"],
    "VendorConfigs": {
      "Statics": {
        "RequireBaseTypes": true,
        "GenerateRepositories": true
      }
    },
    "Discovery": {
      "Methods": ["MetaTypes.Attribute", "Statics.Attribute", "EfCore.DbContextSet"]
    }
  }
}
```

## Performance Considerations

### Repository Performance
- All repositories use singleton pattern for minimal memory usage
- Async consistency adds minimal overhead (`Task.FromResult` for sync methods)
- Repository methods have direct static method calls (no reflection)

### Metadata Performance
- Service method metadata generated at compile-time
- Singleton pattern for all metadata instances
- Lazy initialization for optimal startup performance

### Memory Efficiency
- Parameter and attribute collections only created when methods have them
- Shared attribute argument instances where possible
- Minimal memory overhead per service method

## Thread Safety

All Statics extensions are fully thread-safe:
- Repository instances are thread-safe singletons
- Service method metadata is immutable after generation
- Parameter and attribute metadata is read-only
- Safe for concurrent access from multiple threads

## Limitations and Future Enhancements

### Current Limitations
- Repository generation is Statics-only (doesn't integrate with EfCore DbContext yet)
- Complex attribute arguments may not be fully captured
- No support for generic static methods
- Repository methods don't support cancellation tokens

### Planned Enhancements
- EfCore integration for unified repositories (combining static methods with DbContext operations)
- Full attribute argument capture with complex types
- Generic method support
- Cancellation token support in repository methods
- Advanced service method filtering and grouping

## Troubleshooting

### Statics Extensions Not Generated

**Common Issues:**
1. **Missing Vendor Registration**: Ensure `"Statics"` is in `EnabledVendors`
2. **Missing Discovery Method**: Include `"Statics.Attribute"` in discovery methods
3. **No Attributed Methods**: Verify static methods have `[StaticsServiceMethod]` attribute
4. **Missing MetaType**: Static classes must be marked with `[MetaType]`

### Repository Generation Issues

**Check:**
- `StaticsServiceMethod` attributes have correct `Entity` parameters
- Service methods return consistent result types
- Static classes are properly discovered and have MetaTypes generated
- Base MetaTypes generation is enabled if `RequireBaseTypes: true`

### Service Method Discovery Issues

**Verify:**
- Methods are `public static`
- Methods have `[StaticsServiceMethod]` attribute
- Containing class is `static` and marked with `[MetaType]`
- Method signatures use supported types for parameters and return values

**Enable Diagnostics:**
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true
  }
}
```

Look for validation diagnostics in `_StaticsValidationDiagnostic.g.cs`.