# EfCore Extensions

When Entity Framework Core is detected in your project, MetaTypes automatically generates additional partial class extensions that implement EfCore-specific interfaces. These extensions provide rich metadata about database entities, including keys, foreign keys, table names, and unmapped properties.

## Overview

The EfCore extensions enhance the core MetaType and MetaTypeMember classes with database-specific metadata:

- **IMetaTypeEfCore**: Adds table names and primary key collections to MetaType classes
- **IMetaTypeMemberEfCore**: Adds key detection, foreign key relationships, and unmapped property identification to MetaTypeMember classes

## Auto-Detection

EfCore extensions are automatically enabled when:
1. `Microsoft.EntityFrameworkCore` package is referenced in your project
2. EfCore detection is not explicitly disabled in configuration
3. At least one type is discovered through DbContext scanning

**Configuration Control:**
```json
{
  "EfCoreDetection": true,  // Force enable/disable
  "DiscoveryMethods": {
    "EfCore": {
      "DbContextBased": true,   // Scan DbContext entities
      "EntityBased": true       // Include EF attributes on all types
    }
  }
}
```

## IMetaTypeEfCore Interface

### Interface Definition

```csharp
public interface IMetaTypeEfCore
{
    string? TableName { get; }
    IReadOnlyList<IMetaTypeMemberEfCore> Keys { get; }
}
```

### Generated Implementation

```csharp
public partial class CustomerMetaType : IMetaType, IMetaType<Customer>, IMetaTypeEfCore
{
    // EfCore-specific properties
    public string? TableName => "Customers";
    
    public IReadOnlyList<IMetaTypeMemberEfCore> Keys => [
        CustomerMetaTypeMemberId.Instance,
        // ... other key members
    ];
}
```

### Properties

#### TableName Property

| Property | Type | Description |
|----------|------|-------------|
| `TableName` | `string?` | The database table name for this entity |

**Table Name Resolution:**
1. **Explicit [Table] Attribute**: Uses the name from `[Table("CustomTableName")]`
2. **DbContext Configuration**: Resolves from DbContext entity configuration
3. **EF Conventions**: Uses EF Core's default naming conventions
4. **Pluralization**: Automatically pluralizes type names (Customer → Customers)

**Examples:**
```csharp
// Explicit table attribute
[MetaType]
[Table("customer_records")]
public class Customer { }
// TableName => "customer_records"

// Convention-based
[MetaType]
public class Customer { }
// TableName => "Customers"

// DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>().ToTable("CustomerData");
}
// TableName => "CustomerData"
```

#### Keys Collection

| Property | Type | Description |
|----------|------|-------------|
| `Keys` | `IReadOnlyList<IMetaTypeMemberEfCore>` | Collection of primary key members |

**Key Detection Logic:**
1. **[Key] Attribute**: Properties explicitly marked with `[Key]`
2. **EF Conventions**: Properties named "Id" or "{TypeName}Id"
3. **DbContext Configuration**: Keys configured in `OnModelCreating`
4. **Composite Keys**: Multiple properties forming a composite key

**Examples:**
```csharp
// Explicit key attribute
[MetaType]
public class Customer
{
    [Key]
    public int CustomerId { get; set; }
}
// Keys collection contains CustomerId

// Convention-based key
[MetaType]
public class Customer
{
    public int Id { get; set; }  // Detected as key by convention
}
// Keys collection contains Id

// Composite key
[MetaType]
public class OrderLine
{
    [Key, Column(Order = 0)]
    public int OrderId { get; set; }
    
    [Key, Column(Order = 1)]
    public int LineNumber { get; set; }
}
// Keys collection contains OrderId and LineNumber
```

## IMetaTypeMemberEfCore Interface

### Interface Definition

```csharp
public interface IMetaTypeMemberEfCore
{
    bool IsKey { get; }
    bool IsForeignKey { get; }
    bool IsNotMapped { get; }
    IMetaTypeMember? ForeignKeyMember { get; }
}
```

### Generated Implementation

```csharp
public partial class CustomerMetaTypeMemberId : IMetaTypeMember, IMetaTypeMemberEfCore
{
    // EfCore-specific properties
    public bool IsKey => true;
    public bool IsForeignKey => false;
    public bool IsNotMapped => false;
    public IMetaTypeMember? ForeignKeyMember => null;
}

public partial class CustomerMetaTypeMemberAddressId : IMetaTypeMember, IMetaTypeMemberEfCore
{
    public bool IsKey => false;
    public bool IsForeignKey => true;
    public bool IsNotMapped => false;
    public IMetaTypeMember? ForeignKeyMember => null; // Could reference navigation property
}
```

### Properties

#### IsKey Property

| Property | Type | Description |
|----------|------|-------------|
| `IsKey` | `bool` | Whether this property is part of the primary key |

**Key Detection:**
- **[Key] Attribute**: Explicitly marked primary key properties
- **Convention**: Properties named "Id" or "{TypeName}Id"
- **Composite Keys**: Multiple properties can have `IsKey = true`

#### IsForeignKey Property  

| Property | Type | Description |
|----------|------|-------------|
| `IsForeignKey` | `bool` | Whether this property represents a foreign key |

**Foreign Key Detection:**
- **[ForeignKey] Attribute**: Explicitly marked foreign key properties
- **Convention**: Properties ending in "Id" that reference other entities
- **Navigation Properties**: Properties that correspond to navigation properties

**Foreign Key Examples:**
```csharp
[MetaType]
public class Order
{
    public int Id { get; set; }
    
    public int CustomerId { get; set; }  // Foreign key by convention
    public Customer Customer { get; set; } = null!;  // Navigation property
    
    [ForeignKey("Address")]
    public int ShippingAddressId { get; set; }  // Explicit foreign key
    public Address Address { get; set; } = null!;
}

// Generated members
public partial class OrderMetaTypeMemberCustomerId : IMetaTypeMemberEfCore
{
    public bool IsForeignKey => true;  // Detected by convention
}

public partial class OrderMetaTypeMemberShippingAddressId : IMetaTypeMemberEfCore
{
    public bool IsForeignKey => true;  // Detected by attribute
}
```

#### IsNotMapped Property

| Property | Type | Description |
|----------|------|-------------|
| `IsNotMapped` | `bool` | Whether this property is excluded from database mapping |

**NotMapped Detection:**
- **[NotMapped] Attribute**: Properties explicitly marked as not mapped
- **Computed Properties**: Read-only properties that are calculated fields
- **Navigation Properties**: May be marked as not mapped in certain scenarios

**NotMapped Examples:**
```csharp
[MetaType]
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";  // Computed property
    
    [NotMapped]
    public int Age { get; set; }  // Calculated elsewhere, not stored
}

// Generated members
public partial class CustomerMetaTypeMemberFullName : IMetaTypeMemberEfCore
{
    public bool IsNotMapped => true;  // Has [NotMapped] attribute
}

public partial class CustomerMetaTypeMemberAge : IMetaTypeMemberEfCore
{
    public bool IsNotMapped => true;  // Has [NotMapped] attribute
}
```

#### ForeignKeyMember Property

| Property | Type | Description |
|----------|------|-------------|
| `ForeignKeyMember` | `IMetaTypeMember?` | Reference to the corresponding navigation property member |

**Note**: This property is currently implemented as `null` in the current version but is reserved for future enhancements to link foreign key properties with their navigation properties.

## Usage Patterns

### Entity Metadata Inspection

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

// Check if EfCore metadata is available
if (mtCustomer is IMetaTypeEfCore efCoreType)
{
    Console.WriteLine($"Entity: {efCoreType.TableName}");
    Console.WriteLine($"Primary Keys: {efCoreType.Keys.Count}");
    
    // List all primary key properties
    foreach (var key in efCoreType.Keys)
    {
        Console.WriteLine($"  Key: {key.MemberName} ({key.MemberType.Name})");
    }
}
```

### Property Analysis

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

Console.WriteLine("Property Analysis:");
foreach (var member in mtCustomer.Members)
{
    if (member is IMetaTypeMemberEfCore efCoreMember)
    {
        var flags = new List<string>();
        
        if (efCoreMember.IsKey) flags.Add("KEY");
        if (efCoreMember.IsForeignKey) flags.Add("FK");
        if (efCoreMember.IsNotMapped) flags.Add("NOT_MAPPED");
        
        var flagsStr = flags.Any() ? $" [{string.Join(", ", flags)}]" : "";
        Console.WriteLine($"  {member.MemberName} ({member.MemberType.Name}){flagsStr}");
    }
}
```

### Database Schema Generation

```csharp
public class SchemaAnalyzer
{
    private readonly IServiceProvider _serviceProvider;
    
    public SchemaAnalyzer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void AnalyzeSchema()
    {
        var providers = _serviceProvider.GetServices<IMetaTypeProvider>();
        
        foreach (var provider in providers)
        {
            foreach (var metaType in provider.AssemblyMetaTypes)
            {
                if (metaType is IMetaTypeEfCore efCoreType)
                {
                    AnalyzeEntity(efCoreType);
                }
            }
        }
    }
    
    private void AnalyzeEntity(IMetaTypeEfCore entity)
    {
        Console.WriteLine($"Table: {entity.TableName}");
        
        // Primary Keys
        if (entity.Keys.Any())
        {
            Console.WriteLine($"  Primary Key: {string.Join(", ", entity.Keys.Select(k => k.MemberName))}");
        }
        
        // Columns
        foreach (var member in entity.Keys.Cast<IMetaTypeMember>().Concat(
                    ((IMetaType)entity).Members.Where(m => m is IMetaTypeMemberEfCore efCore && !efCore.IsNotMapped)))
        {
            if (member is IMetaTypeMemberEfCore efCoreMember)
            {
                var columnInfo = $"    {member.MemberName} {GetSqlType(member.MemberType)}";
                
                if (efCoreMember.IsKey) columnInfo += " PRIMARY KEY";
                if (efCoreMember.IsForeignKey) columnInfo += " FOREIGN KEY";
                if (!member.HasSetter) columnInfo += " READONLY";
                
                Console.WriteLine(columnInfo);
            }
        }
        
        Console.WriteLine();
    }
    
    private string GetSqlType(Type type)
    {
        // Simplified type mapping
        return type.Name switch
        {
            "Int32" => "INT",
            "String" => "NVARCHAR(MAX)",
            "DateTime" => "DATETIME2",
            "Boolean" => "BIT",
            "Decimal" => "DECIMAL(18,2)",
            _ => "UNKNOWN"
        };
    }
}
```

### Foreign Key Relationship Mapping

```csharp
public class RelationshipMapper
{
    public void MapRelationships(IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<IMetaTypeProvider>();
        var allTypes = providers.SelectMany(p => p.AssemblyMetaTypes).ToList();
        
        foreach (var metaType in allTypes)
        {
            Console.WriteLine($"Entity: {metaType.ManagedTypeName}");
            
            foreach (var member in metaType.Members)
            {
                if (member is IMetaTypeMemberEfCore efCoreMember && efCoreMember.IsForeignKey)
                {
                    Console.WriteLine($"  Foreign Key: {member.MemberName}");
                    
                    // Try to find the referenced entity
                    var referenceProp = metaType.Members
                        .FirstOrDefault(m => m.IsMetaType && 
                                           m.MemberName.Equals(member.MemberName.Replace("Id", ""), 
                                                             StringComparison.OrdinalIgnoreCase));
                    
                    if (referenceProp?.MetaType != null)
                    {
                        Console.WriteLine($"    References: {referenceProp.MetaType.ManagedTypeName}");
                    }
                }
            }
            
            Console.WriteLine();
        }
    }
}
```

## Configuration Examples

### Minimal EfCore Configuration

```json
{
  "EfCoreDetection": true
}
```

This enables basic EfCore features with auto-detection of DbContext entities.

### Full EfCore Configuration

```json
{
  "EfCoreDetection": true,
  "DiagnosticFiles": true,
  "DiscoveryMethods": {
    "Common": {
      "AttributeBased": true,
      "ReferencedTypes": true
    },
    "EfCore": {
      "DbContextBased": true,
      "EntityBased": true
    }
  }
}
```

This enables all EfCore features including:
- DbContext entity scanning
- Attribute-based entity detection on all types
- Cross-reference discovery
- Diagnostic file generation

### MSBuild Configuration

```xml
<PropertyGroup>
  <MetaTypeEfCoreDetection>true</MetaTypeEfCoreDetection>
  <MetaTypeDiagnosticFiles>true</MetaTypeDiagnosticFiles>
</PropertyGroup>

<ItemGroup>
  <CompilerVisibleItemMetadata Include="MetaTypeConfig" MetadataName="DiscoveryMethod" />
  <MetaTypeConfig Include="EfCore.DbContextBased" DiscoveryMethod="true" />
  <MetaTypeConfig Include="EfCore.EntityBased" DiscoveryMethod="true" />
</ItemGroup>
```

## Performance Considerations

### Lazy Evaluation
- EfCore metadata is generated at compile-time
- No runtime DbContext inspection
- All properties use compile-time constants

### Memory Efficiency  
- Keys collection only created for entities with primary keys
- Boolean properties use compile-time constants
- No additional memory overhead for non-EfCore scenarios

### Thread Safety
- All EfCore extensions are thread-safe
- Properties return immutable data
- Safe for concurrent access

## Limitations and Future Enhancements

### Current Limitations
- `ForeignKeyMember` property is not yet implemented
- Complex key relationships are simplified
- Attribute parameters are not captured for complex attributes
- No support for owned entity types or value objects

### Planned Enhancements
- Full navigation property linking
- Support for owned entity types
- Enhanced foreign key relationship mapping
- Integration with EF Core model metadata
- Support for custom conventions and configurations

## Troubleshooting

### EfCore Extensions Not Generated

**Check these common issues:**

1. **Missing EfCore Reference**: Ensure `Microsoft.EntityFrameworkCore` is referenced
2. **Configuration Disabled**: Check that `EfCoreDetection` is not set to `false`
3. **No Entities Found**: Verify that entities are marked with `[MetaType]` or discovered through DbContext
4. **Build Issues**: Clean and rebuild the project

**Diagnostic Information:**
Enable diagnostic files to see EfCore detection status:
```json
{
  "DiagnosticFiles": true
}
```

Look for lines like:
```
EfCore Detection Enabled: True
Discovery Methods: 4 (Common: 2, EfCore: 2)
```

### Incorrect Key Detection

**Common issues:**
- Properties not following EF naming conventions
- Missing `[Key]` attributes for non-conventional keys
- DbContext configuration not being detected

**Solutions:**
- Use explicit `[Key]` attributes
- Ensure property names follow "Id" or "{TypeName}Id" conventions
- Verify DbContext entity configurations are applied

### Missing Foreign Key Detection

**Check:**
- Property names end with "Id"
- Corresponding navigation properties exist
- `[ForeignKey]` attributes are correctly applied
- DbContext has proper relationship configurations