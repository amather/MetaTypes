# MetaTypes Provider

The MetaTypes Provider is an assembly-level class that serves as the central registry for all MetaTypes generated within an assembly. It implements `IMetaTypeProvider` and provides access to all discovered types and their metadata.

## Class Structure

### Generated Provider Pattern

For an assembly containing multiple MetaTypes, the generator creates a single provider:

```csharp
public partial class MetaTypes : IMetaTypeProvider
{
    // Singleton pattern
    private static MetaTypes? _instance;
    public static MetaTypes Instance => _instance ??= new();

    // Assembly registry
    public IReadOnlyList<IMetaType> AssemblyMetaTypes => [
        CustomerMetaType.Instance,
        CustomerAddressMetaType.Instance,
        ProductMetaType.Instance,
        OrderMetaType.Instance,
        // ... all other MetaTypes in this assembly
    ];
}
```

## Properties

### AssemblyMetaTypes Collection

| Property | Type | Description |
|----------|------|-------------|
| `AssemblyMetaTypes` | `IReadOnlyList<IMetaType>` | Collection of all MetaType instances in the current assembly |

**Collection Characteristics:**
- Contains one entry for each type marked with `[MetaType]` in the assembly
- Ordered alphabetically by type name for consistency
- Immutable collection created at compile-time
- All instances are singleton references (no duplication)

## Interface Implementation

### IMetaTypeProvider Interface

```csharp
public interface IMetaTypeProvider
{
    IReadOnlyList<IMetaType> AssemblyMetaTypes { get; }
}
```

The provider implements this interface to enable dependency injection and service registration.

## Namespace and Assembly Name

### Default Naming Convention

By default, the MetaTypes provider class is generated in a namespace determined by:

1. **Configuration Override**: If `AssemblyName` is specified in configuration
2. **Auto-Detection**: The most common namespace prefix among discovered types
3. **Assembly Name Fallback**: The actual assembly name if auto-detection fails

**Examples:**

```csharp
// Configuration: { "AssemblyName": "MyApp.Business" }
namespace MyApp.Business;
public partial class MetaTypes : IMetaTypeProvider { }

// Auto-detected from types: MyApp.Models.Customer, MyApp.Models.Product
namespace MyApp.Models;
public partial class MetaTypes : IMetaTypeProvider { }

// Assembly fallback: MyApp.Business.dll
namespace MyApp.Business;
public partial class MetaTypes : IMetaTypeProvider { }
```

### Custom Assembly Name

Override the namespace using configuration:

**JSON Configuration:**
```json
{
  "AssemblyName": "MyCustomNamespace"
}
```

**MSBuild Configuration:**
```xml
<PropertyGroup>
  <MetaTypeAssemblyName>MyCustomNamespace</MetaTypeAssemblyName>
</PropertyGroup>
```

## Usage Patterns

### Dependency Injection Registration

The primary use case is registering the provider with dependency injection:

```csharp
// Register MetaTypes from multiple assemblies
services.AddMetaTypes<MyApp.Business.MetaTypes>();
services.AddMetaTypes<MyApp.Auth.MetaTypes>();
services.AddMetaTypes<MyApp.Core.MetaTypes>();
```

**Extension Method Implementation:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetaTypes<TMetaTypesProvider>(this IServiceCollection services)
        where TMetaTypesProvider : class, IMetaTypeProvider, new()
    {
        var provider = new TMetaTypesProvider();
        
        // Register the provider instance
        services.AddSingleton<IMetaTypeProvider>(provider);
        
        // Register individual MetaTypes for direct injection
        foreach (var metaType in provider.AssemblyMetaTypes)
        {
            services.AddSingleton(metaType.GetType(), metaType);
            services.AddSingleton(metaType.ManagedType, metaType);
        }
        
        return services;
    }
}
```

### Direct Provider Access

Access the provider directly for assembly-level operations:

```csharp
// Get all MetaTypes in an assembly
var allMetaTypes = MyApp.Business.MetaTypes.Instance.AssemblyMetaTypes;

Console.WriteLine($"Assembly contains {allMetaTypes.Count} MetaTypes:");
foreach (var metaType in allMetaTypes)
{
    Console.WriteLine($"  {metaType.ManagedTypeName} ({metaType.Members.Count} members)");
}
```

### Multi-Assembly Scenarios

When working with multiple assemblies, you can access each provider independently:

```csharp
// Business assembly MetaTypes
var businessTypes = MyApp.Business.MetaTypes.Instance.AssemblyMetaTypes;
Console.WriteLine($"Business Types: {businessTypes.Count}");

// Auth assembly MetaTypes
var authTypes = MyApp.Auth.MetaTypes.Instance.AssemblyMetaTypes;
Console.WriteLine($"Auth Types: {authTypes.Count}");

// Combined access through DI
var combinedProvider = serviceProvider.GetServices<IMetaTypeProvider>();
var allTypes = combinedProvider.SelectMany(p => p.AssemblyMetaTypes).ToList();
Console.WriteLine($"Total Types: {allTypes.Count}");
```

### Type Discovery

Use the provider to discover types dynamically:

```csharp
var provider = MyApp.Business.MetaTypes.Instance;

// Find MetaType by type name
var customerMetaType = provider.AssemblyMetaTypes
    .FirstOrDefault(mt => mt.ManagedTypeName == "Customer");

// Find MetaTypes by namespace
var modelTypes = provider.AssemblyMetaTypes
    .Where(mt => mt.ManagedTypeNamespace.EndsWith(".Models"))
    .ToList();

// Find MetaTypes with specific characteristics
var typesWithCollections = provider.AssemblyMetaTypes
    .Where(mt => mt.Members.Any(m => m.IsList))
    .ToList();

// Find MetaTypes with cross-references
var typesWithReferences = provider.AssemblyMetaTypes
    .Where(mt => mt.Members.Any(m => m.IsMetaType))
    .ToList();
```

### Assembly Metadata

Extract assembly-level information:

```csharp
var provider = MyApp.Business.MetaTypes.Instance;
var metaTypes = provider.AssemblyMetaTypes;

// Assembly statistics
var totalTypes = metaTypes.Count;
var totalMembers = metaTypes.Sum(mt => mt.Members.Count);
var avgMembersPerType = totalMembers / (double)totalTypes;

Console.WriteLine($"Assembly Statistics:");
Console.WriteLine($"  Total Types: {totalTypes}");
Console.WriteLine($"  Total Members: {totalMembers}");
Console.WriteLine($"  Average Members per Type: {avgMembersPerType:F1}");

// Namespace distribution
var namespaceGroups = metaTypes
    .GroupBy(mt => mt.ManagedTypeNamespace)
    .OrderBy(g => g.Key);

Console.WriteLine($"Namespace Distribution:");
foreach (var group in namespaceGroups)
{
    Console.WriteLine($"  {group.Key}: {group.Count()} types");
}

// Cross-reference analysis
var referencedTypes = metaTypes
    .SelectMany(mt => mt.Members)
    .Where(m => m.IsMetaType)
    .Select(m => m.MetaType?.ManagedTypeName)
    .Where(name => name != null)
    .Distinct()
    .ToList();

Console.WriteLine($"Cross-referenced Types: {string.Join(", ", referencedTypes)}");
```

## Performance Considerations

### Singleton Pattern
- The provider uses a thread-safe singleton pattern
- Single instance per assembly, shared across the application
- Zero allocation after first access

### Compile-time Assembly Registry
- The `AssemblyMetaTypes` collection is a compile-time constant array
- No runtime discovery or reflection
- Minimal memory footprint and maximum performance

### Lazy Initialization
- Provider instance created only when first accessed
- MetaType instances are singletons with lazy initialization
- Optimal startup performance

## Thread Safety

The MetaTypes provider is fully thread-safe:
- Singleton instance creation is thread-safe
- `AssemblyMetaTypes` collection is immutable
- All MetaType instances are thread-safe singletons
- Safe for concurrent access from multiple threads

## Integration with Service Provider Extensions

### Service Collection Extensions

```csharp
public static class ServiceProviderExtensions
{
    public static IMetaType<T> GetMetaType<T>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IMetaType<T>>();
    }
    
    public static IMetaType GetMetaType(this IServiceProvider serviceProvider, Type type)
    {
        return serviceProvider.GetRequiredService(type) as IMetaType
            ?? throw new InvalidOperationException($"No MetaType registered for {type.Name}");
    }
}
```

### Usage with DI

```csharp
// Constructor injection
public class CustomerService
{
    private readonly IMetaType<Customer> _customerMetaType;
    
    public CustomerService(IMetaType<Customer> customerMetaType)
    {
        _customerMetaType = customerMetaType;
    }
    
    public void ProcessCustomer(Customer customer)
    {
        foreach (var member in _customerMetaType.Members)
        {
            var value = member.GetValue(customer);
            // Process value...
        }
    }
}

// Service locator pattern
public class DynamicProcessor
{
    private readonly IServiceProvider _serviceProvider;
    
    public DynamicProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ProcessObject<T>(T obj)
    {
        var metaType = _serviceProvider.GetMetaType<T>();
        // Process using MetaType...
    }
}
```

## Diagnostics and Debugging

### Diagnostic Information

When `DiagnosticFiles` is enabled in configuration, the generator produces diagnostic information:

```csharp
// Generated diagnostic file: _MetaTypesGeneratorDiagnostic.g.cs
// Generated by MetaTypes base generator at 7/29/2025 8:00:00 PM
// Assembly: MyApp.Business
// ConfiguredAssemblyName: MyApp.Business
// EfCore Detection Enabled: True
// Diagnostic Files Enabled: True
// Config Keys Found: JSON_CONFIG_LOADED_FROM_/path/to/metatypes.config.json
// Discovery Methods: 4 (Common: 2, EfCore: 2)
// Discovered types: 15
// Discovery breakdown: Referenced-Common: 12, Referenced-EfCore: 3
// Types: Customer (Referenced by Common), CustomerAddress (Referenced by Common), ...
```

### Debugging Provider Registration

```csharp
// Verify provider registration
var providers = serviceProvider.GetServices<IMetaTypeProvider>().ToList();
Console.WriteLine($"Registered Providers: {providers.Count}");

foreach (var provider in providers)
{
    Console.WriteLine($"Provider: {provider.GetType().FullName}");
    Console.WriteLine($"  Types: {provider.AssemblyMetaTypes.Count}");
    
    foreach (var metaType in provider.AssemblyMetaTypes)
    {
        Console.WriteLine($"    {metaType.ManagedTypeFullName}");
    }
}
```

## Best Practices

### Provider Naming
- Keep the default `MetaTypes` class name for consistency
- Use meaningful assembly names in configuration when needed
- Follow namespace conventions that match your project structure

### Registration Order
- Register providers in a consistent order across your application
- Consider registering core/shared assemblies first
- Use multiple registration calls rather than trying to combine providers

### Assembly Organization
- Keep related types in the same assembly for optimal cross-referencing
- Avoid circular dependencies between assemblies
- Consider splitting large assemblies if the provider becomes unwieldy

### Performance Optimization
- Access providers through DI rather than static instances when possible
- Cache MetaType references in long-lived services
- Use the provider's collection for bulk operations rather than individual lookups