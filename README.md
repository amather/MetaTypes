# Disclaimer from a human

This project was vibe coded with claude code and it had a very hard time to understand (i.e. not hallucinate) what .NET source generators can do and what not. 

The current state of the code is: compiling, but missing real-world usage. 

Lot's of code has been reviewed by a human, but only for logic errors, not elegance.

Further development will be done in private repo, this is only a showcase of what's possible. Once the private repo (Statics framework) can be open sourced, this project will have underwent:

- full human code review, including AI instructions to block manipulation of certain files or methods (AI freeze)
- clear documentation and approach for vendors to add to this source generator

Until then, input is welcomed, but consider this repo as a proof of concept, nothing more.

(amather, Aug 13th 2025)

# MetaTypes

A C# source generator that creates compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime. Features a vendor-based architecture that allows to extend the discovery of types and generation of metadata.

## Features

- **Compile-time metadata generation** - Eliminates runtime reflection overhead
- **Expression-based member access** - Type-safe member discovery using lambda expressions  
- **Multi-assembly support** - Works across multiple assemblies in the same solution
- **Dependency injection integration** - Built-in DI extensions for registration
- **Vendor extensibility** - Pluggable vendor system with Entity Framework Core support
- **Performance optimization** - Singleton patterns and compile-time constants

## Quick Start

### Entity Framework Core Integration

1. **Configure your project** with `metatypes.config.json`:

```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "EfCore.TableAttribute"]
    },
    "Vendors": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true
      }
    }
  }
}
```

2. **Mark your Entity Framework entities**:

```csharp
using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

[MetaType]
[Table("Customers")]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
```

3. **Register with dependency injection**:

```csharp
builder.Services.AddMetaTypesMyAppEfCore(); // Generated method
```

4. **Access Entity Framework metadata**:

```csharp
// Access individual entity metadata
var efCoreTypes = serviceProvider.GetEfCoreMetaTypes();
foreach (var entityType in efCoreTypes)
{
    Console.WriteLine($"Table: {entityType.TableName}");
    
    var keys = entityType.Keys;
    Console.WriteLine($"Primary keys: {string.Join(", ", keys.Select(k => ((IMetaTypeMember)k).MemberName))}");
}

// Access entities organized by DbContext
var dbContexts = serviceProvider.GetEfCoreDbContexts();
foreach (var dbContext in dbContexts)
{
    Console.WriteLine($"DbContext: {dbContext.ContextName}");
    foreach (var entityType in dbContext.EntityTypes)
    {
        Console.WriteLine($"  - Entity: {((IMetaType)entityType).ManagedTypeName}");
        Console.WriteLine($"    Table: {entityType.TableName}");
    }
}

// Access specific DbContext metadata
var customerContext = serviceProvider.GetEfCoreDbContext<CustomerDbContext>();
if (customerContext != null)
{
    Console.WriteLine($"Found context: {customerContext.ContextName}");
    Console.WriteLine($"Entity count: {customerContext.EntityTypes.Count()}");
}
```

## Documentation

📚 **[Complete Documentation](./docs/README.md)** - Comprehensive guide covering:
- Project goals and architecture
- Configuration system with defaults and examples
- Generated types with detailed API documentation
- Usage patterns and best practices

### Configuration Reference

**Basic configuration** (metatypes.config.json):
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

**Entity Framework Core vendor**:
```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "EfCore.TableAttribute"]
    },
    "Vendors": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```

**Discovery methods**:
- `MetaTypes.Attribute` - Types marked with `[MetaType]`
- `MetaTypes.Reference` - Referenced types with `[MetaType]`
- `EfCore.TableAttribute` - Entity Framework entities with `[Table]`
- `EfCore.DbContextSet` - Entities referenced in `DbSet<T>` properties

📖 **[Detailed Configuration Documentation](./docs/CONFIG.md)**

**Project file setup**:
```xml
<ItemGroup>
  <AdditionalFiles Include="metatypes.config.json" Type="MetaTypes.Generator.Options" />
  <CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="Type" />
</ItemGroup>
```

## How It Works

The source generator:

1. **Scans for `[MetaType]` attributes** on classes, structs, and records
2. **Generates singleton MetaType classes** for each marked type
3. **Creates an assembly-level provider** that lists all MetaTypes
4. **Produces compile-time constants** for optimal performance

### Generated Code Example

For a `Customer` class, the generator creates:

```csharp
public sealed class CustomerMetaType : IMetaType, IMetaType<Customer>
{
    private static CustomerMetaType? _instance;
    public static CustomerMetaType Instance => _instance ??= new();

    public Type ManagedType => typeof(Customer);
    public string ManagedTypeName => "Customer";
    public string ManagedTypeNamespace => "MyApp.Models";
    // ... other properties

    public IReadOnlyList<IMetaTypeMember> Members => [
        CustomerMetaTypeMemberName.Instance,
        CustomerMetaTypeMemberEmail.Instance,
        // ... other members
    ];
}
```

## Performance

MetaTypes eliminates runtime reflection by:

- Using **compile-time constants** for all metadata
- Implementing **thread-safe singletons** for zero allocation
- Leveraging **expression trees** for type-safe member access
- Generating **optimized code** with modern C# features

## Multi-Assembly Support

MetaTypes works seamlessly across multiple assemblies:

```csharp
// Assembly 1: MyApp.Business
namespace MyApp.Business.Models;

[MetaType]
public class Customer { /* ... */ }

// Assembly 2: MyApp.Auth  
namespace MyApp.Auth.Models;

[MetaType]
public class User { /* ... */ }

// Consumer application
services.AddMetaTypes<MyApp.Business.MetaTypes>();
services.AddMetaTypes<MyApp.Auth.MetaTypes>();
```

## Supported Types

- ✅ Classes
- ✅ Records (both class and struct)
- ✅ Structs
- ✅ Generic types
- ✅ Nullable reference types
- ✅ Properties with attributes
- ✅ Collections (with `IsList` detection)

## Building from Source

```bash
git clone https://github.com/your-org/MetaTypes.git
cd MetaTypes
dotnet build
dotnet test
```

### Run the samples

**Entity Framework Core vendor** (primary showcase):
```bash
cd samples/Vendor/EfCore/Sample.EfCore.SingleProject
dotnet run
```

**Basic MetaTypes generation**:
```bash
cd samples/Sample.Console
dotnet run
```

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to the main branch.

## License

This project is licensed under the MIT License - see the LICENSE file for details.