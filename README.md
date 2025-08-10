# MetaTypes

A unified C# source generator that creates compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime. Features a vendor-based architecture with Entity Framework Core extensions.

> **✅ Fully Functional** - Vendor-based architecture implemented and tested (August 10th, 2025)

## Features

- 🚀 **Zero runtime reflection** - All metadata is generated at compile time
- 🔍 **Expression-based member access** - Type-safe member discovery using lambda expressions
- 🏗️ **Multi-assembly support** - Works across multiple assemblies in the same solution
- 🔌 **Dependency Injection integration** - Built-in DI extensions for easy registration
- 📦 **Modern C#** - Built with .NET 8+, C# 13, and nullable reference types
- ⚡ **Performance optimized** - Singleton patterns and compile-time constants
- 🔌 **Vendor Extensions** - Pluggable vendor system with Entity Framework Core support
- 🎯 **Unified Architecture** - Single generator with vendor plugins for specialized metadata

## Quick Start

### 1. Install the packages

```xml
<ItemGroup>
  <PackageReference Include="MetaTypes.Abstractions" Version="1.0.0" />
  <PackageReference Include="MetaTypes.Generator" Version="1.0.0" PrivateAssets="all" />
</ItemGroup>
```

### 2. Mark your types

```csharp
using MetaTypes.Abstractions;

[MetaType]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public List<CustomerAddress> Addresses { get; set; } = [];
}
```

### 3. Register with DI

```csharp
using MetaTypes.Abstractions;

var builder = Host.CreateApplicationBuilder(args);

// Register MetaTypes from your assemblies
builder.Services.AddMetaTypes<MyApp.Business.MetaTypes>();
builder.Services.AddMetaTypes<MyApp.Auth.MetaTypes>();

var host = builder.Build();
```

### 4. Use the MetaTypes

```csharp
// Get MetaType for a specific type
var customerMetaType = serviceProvider.GetMetaType<Customer>();

Console.WriteLine($"Type: {customerMetaType.ManagedTypeName}");
Console.WriteLine($"Namespace: {customerMetaType.ManagedTypeNamespace}");
Console.WriteLine($"Members: {customerMetaType.Members.Count}");

// Find members by name
var nameProperty = customerMetaType.FindMember("Name");
Console.WriteLine($"Name property type: {nameProperty?.MemberType.Name}");

// Find members by expression (type-safe)
var emailProperty = customerMetaType.FindMember<string>(c => c.Email);
Console.WriteLine($"Email property: {emailProperty?.MemberName}");

// Iterate through all members
foreach (var member in customerMetaType.Members)
{
    Console.WriteLine($"{member.MemberName} ({member.MemberType.Name}) - " +
                     $"HasSetter: {member.HasSetter}, IsList: {member.IsList}");
}
```

## Documentation

📚 **[Complete Documentation](./docs/README.md)** - Comprehensive guide covering:
- Project goals and architecture
- Configuration system with defaults and examples
- Generated types with detailed API documentation
- Usage patterns and best practices

### Quick Configuration

**Basic project** (metatypes.config.json):
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    }
  }
}
```

**With Entity Framework Core**:
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": [
        "MetaTypes.Attribute", 
        "MetaTypes.Reference",
        "EfCore.TableAttribute",
        "EfCore.DbContextSet"
      ]
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

**Basic MetaTypes generation:**
```bash
cd samples/Sample.Console
dotnet run
```

**Entity Framework Core vendor generation:**
```bash
cd samples/Vendor/EfCore/Sample.EfCore.LocalOnly
dotnet run
```

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to the main branch.

## License

This project is licensed under the MIT License - see the LICENSE file for details.