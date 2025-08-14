# EfCore Vendor Sample

## Overview
This folder contains samples demonstrating the Entity Framework Core vendor integration with MetaTypes. The EfCore vendor extends the base MetaTypes system with EF-specific metadata like table names, primary keys, foreign keys, and **DbContext collections**.

## Samples

### Single Project Sample
**Location**: `Sample.EfCore.SingleProject/`
**Purpose**: Demonstrates EfCore vendor features in a single-project setup

```bash
cd Sample.EfCore.SingleProject
dotnet run
```

### Multi-Project Sample  
**Location**: `Sample.EfCore.MultiProject/`
**Purpose**: Shows cross-assembly EfCore metadata generation across multiple projects

```bash
cd Sample.EfCore.MultiProject
dotnet run
```

## EfCore DbContext Collection Features

The EfCore vendor organizes entity types by their DbContext, enabling consumers to iterate through DbContexts and access their associated entity types.

### IMetaTypesEfCoreDbContext Interface

```csharp
public interface IMetaTypesEfCoreDbContext
{
    string ContextName { get; }           // DbContext class name
    Type ContextType { get; }             // DbContext Type
    IEnumerable<IMetaTypeEfCore> EntityTypes { get; }  // Entities in this context
}
```

### Generated DbContext Collections

For each discovered DbContext, the generator creates:

```csharp
public class CustomerDbContextMetaTypesEfCoreDbContext : IMetaTypesEfCoreDbContext
{
    public string ContextName => "CustomerDbContext";
    public Type ContextType => typeof(CustomerDbContext);
    public IEnumerable<IMetaTypeEfCore> EntityTypes => /* filtered entities */;
}
```

### DI Service Provider Extensions

```csharp
// Get all DbContext collections
IEnumerable<IMetaTypesEfCoreDbContext> GetEfCoreDbContexts(this IServiceProvider)

// Get specific DbContext by type
IMetaTypesEfCoreDbContext? GetEfCoreDbContext<TDbContext>(this IServiceProvider)
IMetaTypesEfCoreDbContext? GetEfCoreDbContext(this IServiceProvider, Type dbContextType)
```

### Usage Patterns

```csharp
// Iterate through DbContexts and their entities
foreach (var dbContext in serviceProvider.GetEfCoreDbContexts())
{
    Console.WriteLine($"DbContext: {dbContext.ContextName}");
    foreach (var entityType in dbContext.EntityTypes)
    {
        Console.WriteLine($"  Entity: {((IMetaType)entityType).ManagedTypeName}");
        Console.WriteLine($"  Table: {entityType.TableName}");
        Console.WriteLine($"  Keys: {string.Join(", ", entityType.Keys.Select(k => k.MemberName))}");
    }
}

// Access specific DbContext
var customerContext = serviceProvider.GetEfCoreDbContext<CustomerDbContext>();
if (customerContext != null)
{
    var customerEntities = customerContext.EntityTypes.ToList();
    Console.WriteLine($"Found {customerEntities.Count} entities in {customerContext.ContextName}");
}
```

### Entity Discovery and Context Grouping

Entities are grouped by their discovery method:
- **DbContext Scanning**: Entities discovered via `DbSet<T>` properties are grouped by their actual DbContext
- **Table Attribute**: Entities discovered via `[Table]` attribute are grouped in an "UnknownContext" since they could belong to multiple DbContexts

### Future Extensibility

The framework is designed to support future `ConfigureModel` method generation:

```csharp
// Future enhancement
foreach (var dbContext in serviceProvider.GetEfCoreDbContexts())
{
    // dbContext.ConfigureModel(modelBuilder);  // Future feature
}
```

## Configuration Examples

### Basic EfCore Vendor Configuration

```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["EfCore.TableAttribute", "EfCore.DbContextSet"]
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

### Discovery Methods
- `EfCore.TableAttribute` - Discovers entities with `[Table]` attribute
- `EfCore.DbContextSet` - Discovers entities referenced in `DbSet<T>` properties

## Generated Code Structure

For each EfCore entity type, the generator creates:
- **Base MetaType**: `EntityNameMetaType.g.cs` (if `BaseMetaTypes: true`)
- **EfCore Extension**: `EntityNameMetaTypeEfCore.g.cs` (partial class with `IMetaTypeEfCore`)
- **DbContext Collections**: `{ContextName}MetaTypesEfCoreDbContext.g.cs` (per discovered DbContext)
- **DI Extensions**: `EfCoreServiceCollectionExtensions.g.cs` (registration methods)

## Key Features

✅ **Table Metadata** - Table names from `[Table]` attributes or conventions  
✅ **Primary Key Detection** - Via `[Key]` attributes or Id/EntityNameId conventions  
✅ **Foreign Key Support** - Detection of `[ForeignKey]` relationships  
✅ **DbContext Organization** - Entities grouped by their originating DbContext  
✅ **DI Integration** - Service provider extensions for metadata retrieval  
✅ **Cross-Assembly Support** - Works across multiple assemblies  

## Implementation Details

The EfCore vendor is implemented in:
- **Discovery**: `src/MetaTypes.Generator.Common/Vendor/EfCore/Discovery/`
- **Generation**: `src/MetaTypes.Generator.Common/Vendor/EfCore/Generation/`
- **Abstractions**: `src/MetaTypes.Abstractions/Vendor/EfCore/`