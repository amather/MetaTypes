# Entity Framework Core Vendor Samples

This directory contains sample projects demonstrating the EfCore vendor extensions for MetaTypes generation.

## ✅ Status: Fully Working (August 10th, 2025)

All EfCore vendor functionality has been implemented and tested successfully.

## Sample Projects

### Sample.EfCore.LocalOnly
**Purpose**: Demonstrates full EfCore vendor generation with local database setup.

**Features**:
- ✅ EfCore discovery methods (`EfCore.TableAttribute`, `EfCore.DbContextSet`)
- ✅ Vendor generation of `IMetaTypeEfCore` extensions
- ✅ Database creation and seeding
- ✅ Integration with base MetaTypes

**Generated Files**:
- `TestEntityMetaType.g.cs` - Base MetaType class
- `TestEntityEfCoreMetaType.g.cs` - EfCore vendor extension with table metadata

**Configuration**: Full EfCore vendor configuration with all discovery methods enabled.

### Sample.EfCore.Infrastructure  
**Purpose**: Infrastructure-only project with EfCore entities for reference by other projects.

**Features**:
- ✅ Basic MetaTypes generation
- ✅ `[Table]` attribute support
- ✅ Shared entity definitions

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

### Full EfCore Configuration (LocalOnly)
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

### Infrastructure Configuration
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": false
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
# LocalOnly sample
cd Sample.EfCore.LocalOnly
dotnet run

# Infrastructure sample  
cd Sample.EfCore.Infrastructure
dotnet build
```

### Expected Output
The LocalOnly sample will:
1. Create a SQLite database (`local.db`)
2. Apply EF migrations  
3. Display generated MetaTypes information
4. Demonstrate EfCore vendor extensions

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

- **Vendor Generation**: EfCore vendor runs after base generation
- **Partial Classes**: Vendor extensions use partial classes in the same namespace
- **Discovery Filtering**: Vendor generator only processes types discovered by `EfCore.*` methods
- **Interface Extensions**: Generated classes implement both `IMetaType` (base) and `IMetaTypeEfCore` (vendor)