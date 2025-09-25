# MetaTypes Documentation

Welcome to the comprehensive documentation for MetaTypes - a C# source generator that creates compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime.

## Table of Contents

- [Project Overview](#project-overview)
- [Configuration System](#configuration-system)
- [Generated Types Documentation](#generated-types-documentation)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)

## Project Overview

### Goals and Features

MetaTypes aims to provide a **zero-runtime-reflection** solution for type metadata and dynamic property access. The key goals are:

🚀 **Performance**: Eliminate reflection overhead by generating compile-time metadata
🔍 **Type Safety**: Provide expression-based member discovery with full IntelliSense support
🏗️ **Scalability**: Support multi-assembly projects with cross-assembly type references
🔌 **Integration**: Seamless dependency injection and framework integration
📦 **Modern C#**: Built with latest C# features including nullable reference types
⚡ **Developer Experience**: Rich metadata with attributes, EF Core integration, and dynamic property access

### Core Features

1. **Compile-time Type Discovery**: Automatically discovers types marked with `[MetaType]` attribute
2. **Cross-Assembly References**: Detects relationships between types across different assemblies
3. **Dynamic Property Access**: Type-safe `GetValue(obj)` and `SetValue(obj, value)` methods
4. **EF Core Integration**: Built-in support for Entity Framework Core metadata (keys, foreign keys, table names)
5. **Attribute Preservation**: Captures and exposes property and type attributes
6. **Collection Detection**: Automatically identifies list/collection properties
7. **Expression-based Member Finding**: Type-safe member discovery using lambda expressions
8. **Dependency Injection**: Built-in DI extensions for easy service registration

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        MetaTypes Architecture                   │
├─────────────────────────────────────────────────────────────────┤
│  Source Code                                                    │
│  ┌─────────────────┐    ┌─────────────────┐                     │
│  │ [MetaType]      │    │ Configuration   │                     │
│  │ public class    │    │ metatypes.json  │                     │
│  │ Customer {...}  │    │ MSBuild props   │                     │
│  └─────────────────┘    └─────────────────┘                     │
│           │                       │                             │
│           v                       v                             │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Source Generators                              │ │
│  │  ┌─────────────────┐  ┌─────────────────────────────────────┐│ │
│  │  │ Core Generator  │  │ EfCore Extension Generator          ││ │
│  │  │ - Type Discovery│  │ - Key Detection                     ││ │
│  │  │ - MetaTypes     │  │ - Foreign Key Detection             ││ │
│  │  │ - Cross-refs    │  │ - Table Name Resolution             ││ │
│  │  └─────────────────┘  └─────────────────────────────────────┘│ │
│  └─────────────────────────────────────────────────────────────┘ │
│           │                                                     │
│           v                                                     │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                  Generated Code                             │ │
│  │  ┌─────────────────┐  ┌─────────────────┐                   │ │
│  │  │ CustomerMetaType│  │ CustomerEfCore  │                   │ │
│  │  │ - Properties    │  │ - Keys          │                   │ │
│  │  │ - Members       │  │ - TableName     │                   │ │
│  │  │ - GetValue/Set  │  │ - Foreign Keys  │                   │ │
│  │  └─────────────────┘  └─────────────────┘                   │ │
│  └─────────────────────────────────────────────────────────────┘ │
│           │                                                     │
│           v                                                     │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                Runtime Usage                                │ │
│  │  var mt = serviceProvider.GetMetaType<Customer>();          │ │
│  │  var member = mt.FindMember(c => c.Name);                   │ │
│  │  var value = member.GetValue(customerInstance);             │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Configuration System

MetaTypes supports multiple configuration methods with a clear precedence order. Configuration can be provided through JSON files, MSBuild properties, or rely on intelligent defaults.

### Configuration Sources (in order of precedence)

1. **JSON Configuration File** (`metatypes.config.json`)
2. **MSBuild Properties** (in `.csproj` files)
3. **Intelligent Defaults** (based on project structure)

### Default Configuration

When no configuration is provided, MetaTypes uses these defaults:

```json
{
  "MetaTypes.Generator": {
    "Generation": {
      "BaseMetaTypes": true
    },
    "EnabledVendors": [],
    "VendorConfigs": {},
    "Discovery": {
      "CrossAssembly": false,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    },
    "EnableDiagnosticFiles": false
  }
}
```

### JSON Configuration File

Create a `metatypes.config.json` file in your project root:

```json
{
  "MetaTypes.Generator": {
    "Generation": {
      "BaseMetaTypes": true
    },
    "EnabledVendors": ["EfCore", "Statics"],
    "VendorConfigs": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      },
      "Statics": {
        "RequireBaseTypes": true,
        "GenerateRepositories": true
      }
    },
    "Discovery": {
      "CrossAssembly": false,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference", "EfCore.DbContextSet", "Statics.Attribute"]
    },
    "EnableDiagnosticFiles": false
  }
}
```

**Configuration Options:**

- `Generation.BaseMetaTypes`: Generate core MetaType classes (default: true)
- `EnabledVendors`: Array of vendor names to enable (e.g., "EfCore", "Statics")
- `VendorConfigs`: Vendor-specific configuration objects
- `Discovery.CrossAssembly`: Generate unified provider across assemblies (default: false)
- `Discovery.Methods`: Array of discovery method identifiers
- `EnableDiagnosticFiles`: Generate diagnostic files for debugging (default: false)

### MSBuild Configuration

Configure through MSBuild properties in your `.csproj`:

```xml
<PropertyGroup>
  <MetaTypesGenerateBaseTypes>true</MetaTypesGenerateBaseTypes>
  <MetaTypesEnabledVendors>EfCore;Statics</MetaTypesEnabledVendors>
  <MetaTypesCrossAssembly>false</MetaTypesCrossAssembly>
  <MetaTypesDiscoveryMethods>MetaTypes.Attribute;EfCore.DbContextSet;Statics.Attribute</MetaTypesDiscoveryMethods>
  <MetaTypesDiagnosticFiles>false</MetaTypesDiagnosticFiles>
</PropertyGroup>

<ItemGroup>
  <CompilerVisibleItemMetadata Include="MetaTypesVendorConfig" MetadataName="VendorName" />
  <CompilerVisibleItemMetadata Include="MetaTypesVendorConfig" MetadataName="RequireBaseTypes" />
  <MetaTypesVendorConfig Include="EfCore" VendorName="EfCore" RequireBaseTypes="true" />
  <MetaTypesVendorConfig Include="Statics" VendorName="Statics" RequireBaseTypes="true" />
</ItemGroup>
```

### Auto-Detection Logic

MetaTypes includes intelligent auto-detection:

1. **Target Namespace**: Derived from the assembly where the generator runs or most common namespace prefix
2. **Vendor Detection**: EfCore vendor auto-added to `EnabledVendors` when `Microsoft.EntityFrameworkCore` package is referenced
3. **Discovery Methods**: EfCore discovery methods auto-added when EfCore vendor is enabled

### Configuration Validation

The generator validates configuration and provides helpful error messages:

- **Invalid JSON**: Clear parsing error messages with file location
- **Missing Target Namespace**: Warning when auto-detection fails
- **Conflicting Settings**: Warnings when MSBuild and JSON conflict
- **Vendor Dependencies**: Errors when vendor features requested without required package references
- **Discovery Method Issues**: Warnings when discovery methods don't find any types

## Generated Types Documentation

MetaTypes generates several types of classes to provide comprehensive type metadata. Each generated type has specific responsibilities and capabilities:

### Core Generated Types

1. **[MetaType Classes](./generated-types/MetaType.md)** - Core type metadata and member collections
2. **[MetaTypeMember Classes](./generated-types/MetaTypeMember.md)** - Individual property/field metadata with dynamic access
3. **[MetaTypes Provider](./generated-types/MetaTypesProvider.md)** - Assembly-level registry of all MetaTypes
4. **[EfCore Extensions](./generated-types/EfCoreExtensions.md)** - Entity Framework Core specific metadata
5. **[Statics Extensions](./generated-types/StaticsExtensions.md)** - Static service method metadata and repository generation

### Generated Code Structure

For each type marked with `[MetaType]`, the generator creates:

```
CustomerMetaType                    // Main type metadata
├── CustomerMetaTypeMemberId        // Property: Id
├── CustomerMetaTypeMemberName      // Property: Name  
├── CustomerMetaTypeMemberEmail     // Property: Email
└── CustomerMetaTypeMemberAddresses // Property: Addresses

CustomerMetaType (EfCore Extension) // EF Core metadata (if enabled)
├── CustomerMetaTypeMemberId        // EF Core property metadata
├── CustomerMetaTypeMemberName      // EF Core property metadata
└── ...

MetaTypes                          // Assembly provider
└── AssemblyMetaTypes              // Collection of all MetaTypes
```

## Usage Examples

### Basic Usage

```csharp
// Mark types for metadata generation
[MetaType]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public List<CustomerAddress> Addresses { get; set; } = [];
}

// Register with DI
services.AddMetaTypesMyAppBusiness();

// Use at runtime
var mtCustomer = serviceProvider.GetMetaType<Customer>();
Console.WriteLine($"Type: {mtCustomer.ManagedTypeName}");
Console.WriteLine($"Members: {mtCustomer.Members.Count}");
```

### Dynamic Property Access

```csharp
var customer = new Customer { Id = 42, Name = "John Doe" };

// Get property metadata
var mtmName = mtCustomer.FindMember(c => c.Name);
var mtmId = mtCustomer.FindMember(c => c.Id);

// Dynamic get/set
var name = mtmName.GetValue(customer);           // "John Doe"
var id = mtmId.GetValue(customer);               // 42

mtmName.SetValue(customer, "Jane Smith");        // Changes Name
mtmId.SetValue(customer, 99);                    // Changes Id
```

### Cross-Assembly References

```csharp
// Assembly 1: Business.Models
[MetaType] public class Customer { ... }
[MetaType] public class CustomerAddress { ... }

// Assembly 2: Auth.Models  
[MetaType] public class User { ... }

// Consumer
services.AddMetaTypes<Business.MetaTypes>();
services.AddMetaTypes<Auth.MetaTypes>();

var mtCustomer = serviceProvider.GetMetaType<Customer>();
var mtmAddresses = mtCustomer.FindMember(c => c.Addresses);

// Cross-reference detection
if (mtmAddresses.IsMetaType)
{
    var addressMetaType = mtmAddresses.MetaType; // CustomerAddressMetaType.Instance
    Console.WriteLine($"Addresses collection type: {addressMetaType.ManagedTypeName}");
}
```

### EfCore Integration

```csharp
// Entity with EF Core attributes
[MetaType]
[Table("Customers")]
public class Customer
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = "";
    
    [NotMapped]
    public string ComputedField => $"{Name} ({Id})";
    
    public List<CustomerAddress> Addresses { get; set; } = [];
}

// Usage
var mtCustomer = serviceProvider.GetMetaType<Customer>();

// EfCore metadata (if enabled)
if (mtCustomer is IMetaTypeEfCore efCoreType)
{
    Console.WriteLine($"Table: {efCoreType.TableName}");           // "Customers"
    Console.WriteLine($"Primary Keys: {efCoreType.Keys.Count}");   // 1
    
    foreach (var key in efCoreType.Keys)
    {
        Console.WriteLine($"Key: {key.MemberName}");                // "Id"
    }
}

// Member EfCore metadata
var mtmId = mtCustomer.FindMember(c => c.Id);
if (mtmId is IMetaTypeMemberEfCore efCoreMember)
{
    Console.WriteLine($"Is Key: {efCoreMember.IsKey}");            // true
    Console.WriteLine($"Is Foreign Key: {efCoreMember.IsForeignKey}"); // false
}
```

## Best Practices

### 1. Prefer Expression-based Member Finding

```csharp
// ✅ Preferred - Type-safe, refactor-friendly
var member = metaType.FindMember(c => c.PropertyName);

// ❌ Avoid - String-based, error-prone
var member = metaType.FindMember("PropertyName");
```

### 2. Use Consistent Naming Conventions

```csharp
// ✅ Consistent variable naming
var mtCustomer = serviceProvider.GetMetaType<Customer>();
var mtmCustomerName = mtCustomer.FindMember(c => c.Name);
var mtmCustomerEmail = mtCustomer.FindMember(c => c.Email);
```

### 3. Handle Init-only Properties

```csharp
// Generated code automatically handles init-only properties
public class Customer
{
    public int Id { get; init; }  // SetValue throws InvalidOperationException
    public string Name { get; set; }  // SetValue works normally
}
```

### 4. Configure for Your Needs

```csharp
// Minimal configuration for basic scenarios
{
  "DiscoveryMethods": {
    "Common": { "AttributeBased": true }
  }
}

// Full configuration for complex EfCore scenarios
{
  "EfCoreDetection": true,
  "DiscoveryMethods": {
    "Common": { "AttributeBased": true, "ReferencedTypes": true },
    "EfCore": { "DbContextBased": true, "EntityBased": true }
  }
}
```

### 5. Leverage Dependency Injection

```csharp
// Register all assembly MetaTypes
services.AddMetaTypes<MyApp.Business.MetaTypes>();
services.AddMetaTypes<MyApp.Auth.MetaTypes>();

// Use in services
public class CustomerService
{
    private readonly IMetaType<Customer> _customerMetaType;
    
    public CustomerService(IMetaTypeProvider provider)
    {
        _customerMetaType = provider.GetMetaType<Customer>();
    }
}
```

---

For detailed information about each generated type, see the specific documentation files:

- [MetaType Classes](./generated-types/MetaType.md)
- [MetaTypeMember Classes](./generated-types/MetaTypeMember.md)
- [MetaTypes Provider](./generated-types/MetaTypesProvider.md)
- [EfCore Extensions](./generated-types/EfCoreExtensions.md)