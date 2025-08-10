# Entity Framework Core Vendor Samples

This directory contains sample projects demonstrating the EfCore vendor extensions for MetaTypes generation.

## ✅ Status: Fully Working (August 10th, 2025)

All EfCore vendor functionality has been implemented and tested successfully, including critical bug fixes for vendor independence.

## Sample Projects

### Sample.EfCore.SingleProject
**Purpose**: Self-contained project demonstrating EfCore vendor generation with local entities only.

**Features**:
- ✅ Self-contained: All entities defined locally
- ✅ EfCore discovery methods (`EfCore.TableAttribute`, `EfCore.DbContextSet`)
- ✅ Vendor generation of `IMetaTypeEfCore` extensions
- ✅ Database creation with SQLite
- ✅ CrossAssembly discovery disabled (`CrossAssembly: false`)

**Generated Files**:
- `Sample.EfCore.SingleProject_LocalEntityMetaType.g.cs` - Base MetaType class
- `Sample.EfCore.SingleProject_LocalEntityMetaTypeEfCore.g.cs` - EfCore vendor extension with table metadata

**Configuration**: Full EfCore vendor configuration for self-contained scenarios.

### Sample.EfCore.MultiProject
**Purpose**: Multi-project scenario with cross-assembly discovery and external dependencies.

**Features**:
- ✅ Cross-assembly discovery (`CrossAssembly: true`)
- ✅ External project references (Sample.Auth, Sample.Business, Sample.EfCore.Infrastructure)
- ✅ Vendor-only generation (`BaseMetaTypes: false`)
- ✅ EfCore extensions for entities from multiple assemblies

**Generated Files**:
- `{AssemblyName}_{EntityName}MetaTypeEfCore.g.cs` - Vendor extensions for each discovered entity

**Configuration**: Vendor-only configuration optimized for cross-assembly scenarios.

### Sample.EfCore.Infrastructure  
**Purpose**: Infrastructure-only project with EfCore entities for reference by other projects.

**Features**:
- ✅ Basic MetaTypes generation
- ✅ `[Table]` attribute support
- ✅ Shared entity definitions
- ✅ Referenced by MultiProject sample

**Configuration**: Minimal configuration for infrastructure dependencies.

## Generated EfCore Extensions

When types are discovered by EfCore discovery methods, the vendor generator creates additional partial classes implementing `IMetaTypeEfCore`:

```csharp
public partial class TestEntityMetaType : IMetaTypeEfCore
{
    public string? TableName => "TestEntities";
    
    public IReadOnlyList<IMetaTypeMemberEfCore> Keys => [
        TestEntityMetaTypeMemberId.Instance,
    ];
}

public partial class TestEntityMetaTypeMemberId : IMetaTypeMemberEfCore  
{
    public bool IsKey => true;
    public bool IsForeignKey => false;
    public bool IsNotMapped => false;
    public IMetaTypeMember? ForeignKeyMember => null;
}
```

## Configuration Examples

### Self-Contained EfCore Configuration (SingleProject)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": false,
      "Methods": [
        "EfCore.TableAttribute",
        "EfCore.DbContextSet"
      ]
    },
    "EnabledVendors": ["EfCore"],
    "VendorConfigs": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```

### Multi-Project EfCore Configuration (MultiProject)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": false
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": [
        "EfCore.TableAttribute",
        "EfCore.DbContextSet"
      ]
    },
    "EnabledVendors": ["EfCore"],
    "VendorConfigs": {
      "EfCore": {
        "RequireBaseTypes": false,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```

### Infrastructure Configuration
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": false,
      "Methods": ["MetaTypes.Attribute"]
    }
  }
}
```

## EfCore Discovery Methods

### EfCore.TableAttribute
- **Purpose**: Discovers Entity Framework entities via `[Table]` attribute
- **Type**: Syntax-based discovery
- **Scope**: Current compilation only
- **Example**: `[Table("Users")] public class User { ... }`

### EfCore.DbContextSet
- **Purpose**: Discovers entity types by scanning `DbContext` for `DbSet<T>` properties
- **Type**: Syntax-based discovery  
- **Scope**: Current compilation only
- **Example**: `public DbSet<Customer> Customers { get; set; }`

## Running the Samples

### Prerequisites
- .NET 9.0 SDK (or compatible)
- Entity Framework Core packages

### Build and Run
```bash
# SingleProject sample (self-contained)
cd Sample.EfCore.SingleProject
dotnet run

# MultiProject sample (cross-assembly)
cd Sample.EfCore.MultiProject
dotnet run

# Infrastructure sample (build only)  
cd Sample.EfCore.Infrastructure
dotnet build
```

### Expected Output
Both runnable samples will:
1. Create a SQLite database (`single.db` or `local.db`)
2. Apply EF migrations automatically
3. Display "Database created successfully" message
4. Generate MetaTypes and EfCore vendor extensions

## Debugging

Enable diagnostic files to see discovery results:
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true
  }
}
```

View the diagnostic file at:
```
obj/Debug/net9.0/generated/MetaTypes.Generator/MetaTypes.Generator.MetaTypeSourceGenerator/_MetaTypesGeneratorDiagnostic.g.cs
```

## Architecture Notes

### Vendor Independence (August 2025 Updates)
- **Independent Execution**: Vendor generation runs independently of base generation (critical bug fix)
- **RequireBaseTypes Logic**: EfCore vendor respects `RequireBaseTypes` configuration to prevent compilation errors
- **Vendor-Agnostic Core**: Main generator has no hard-coded vendor knowledge
- **Self-Configuring Vendors**: Each vendor parses its own configuration via `Configure(JsonElement? config)`

### File Generation
- **Naming Convention**: `{AssemblyName}_{TypeName}MetaType{VendorName}.g.cs`
- **Cross-Assembly Safety**: Assembly-prefixed file names prevent collisions
- **Partial Classes**: Vendor extensions use partial classes extending base MetaTypes
- **Interface Extensions**: Generated classes implement both `IMetaType` (base) and `IMetaTypeEfCore` (vendor)

### Discovery and Filtering
- **Discovery Methods**: EfCore vendor processes types discovered by `EfCore.TableAttribute` and `EfCore.DbContextSet` methods
- **Prefix Filtering**: Vendor generator only processes types with `EfCore.*` discovery origin
- **CrossAssembly Support**: Works with both local entities (`CrossAssembly: false`) and external entities (`CrossAssembly: true`)

### Configuration Architecture
- **New Format**: Uses `EnabledVendors` array and `VendorConfigs` dictionary instead of old `Vendors` section
- **Explicit Enablement**: Vendors must be explicitly listed in `EnabledVendors` array
- **Configuration Parsing**: Vendors receive `JsonElement?` and deserialize their own config objects