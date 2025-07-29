# MetaType Classes

MetaType classes are the core generated types that provide comprehensive metadata about your marked types. For each class, struct, or record marked with `[MetaType]`, the generator creates a corresponding MetaType class.

## Class Structure

### Generated Class Pattern

For a type named `Customer`, the generator creates:

```csharp
public partial class CustomerMetaType : IMetaType, IMetaType<Customer>
{
    // Singleton pattern
    private static CustomerMetaType? _instance;
    public static CustomerMetaType Instance => _instance ??= new();
    
    // Type information
    public Type ManagedType => typeof(Customer);
    public string ManagedTypeName => "Customer";
    public string ManagedTypeNamespace => "MyApp.Models";
    public string ManagedTypeAssembly => "MyApp.Business";
    public string ManagedTypeFullName => "MyApp.Models.Customer";
    
    // Generic type arguments (for generic types)
    public Type[]? GenericTypeArguments => null; // or array of types
    
    // Type attributes
    public Attribute[]? Attributes => [
        new TableAttribute("Customers"),
        // ... other attributes
    ];
    
    // Members collection
    public IReadOnlyList<IMetaTypeMember> Members => [
        CustomerMetaTypeMemberId.Instance,
        CustomerMetaTypeMemberName.Instance,
        CustomerMetaTypeMemberEmail.Instance,
        // ... other members
    ];
    
    // Member finding methods
    public IMetaTypeMember? FindMember(string name) { /* ... */ }
    public IMetaTypeMember FindRequiredMember(string name) { /* ... */ }
    public IMetaTypeMember? FindMember<TProperty>(Expression<Func<Customer, TProperty>> expression) { /* ... */ }
    public IMetaTypeMember FindRequiredMember<TProperty>(Expression<Func<Customer, TProperty>> expression) { /* ... */ }
}
```

## Properties

### Core Type Information

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `ManagedType` | `Type` | The actual `Type` object being described | `typeof(Customer)` |
| `ManagedTypeName` | `string` | Simple name of the type | `"Customer"` |
| `ManagedTypeNamespace` | `string` | Namespace of the type | `"MyApp.Models"` |
| `ManagedTypeAssembly` | `string` | Assembly name containing the type | `"MyApp.Business"` |
| `ManagedTypeFullName` | `string` | Fully qualified type name | `"MyApp.Models.Customer"` |

### Generic Type Support

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `GenericTypeArguments` | `Type[]?` | Array of generic type arguments, null for non-generic types | `[typeof(string), typeof(int)]` for `Dictionary<string, int>` |

**Generic Type Example:**

```csharp
[MetaType]
public class Repository<T> where T : class
{
    public List<T> Items { get; set; } = [];
}

// Generated MetaType
public partial class RepositoryMetaType : IMetaType, IMetaType<Repository<T>>
{
    public Type[]? GenericTypeArguments => [typeof(T)];
    // ... other properties
}
```

### Attributes

| Property | Type | Description |
|----------|------|-------------|
| `Attributes` | `Attribute[]?` | Array of attributes applied to the type, excluding `MetaTypeAttribute` |

**Attribute Handling:**

- Only attributes with parameterless constructors are captured
- `MetaTypeAttribute` is automatically excluded
- Complex attributes (like `TableAttribute` with parameters) are skipped for safety
- Returns `null` if no attributes are present

### Members Collection

| Property | Type | Description |
|----------|------|-------------|
| `Members` | `IReadOnlyList<IMetaTypeMember>` | Collection of all public instance properties |

**Member Selection Criteria:**
- Only `public` properties are included
- Only instance properties (no `static` properties)
- Both read-only and read-write properties are included
- Indexers are excluded

## Methods

### String-based Member Finding

```csharp
public IMetaTypeMember? FindMember(string name)
public IMetaTypeMember FindRequiredMember(string name)
```

**FindMember(string name)**
- Returns the member with the specified name, or `null` if not found
- Case-sensitive string matching
- Use for dynamic scenarios where property names come from external sources

**FindRequiredMember(string name)**
- Returns the member with the specified name
- Throws `InvalidOperationException` if not found
- Use when you expect the member to exist

**Example:**
```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

// Safe lookup
var member = mtCustomer.FindMember("Name");
if (member != null)
{
    Console.WriteLine($"Found: {member.MemberName}");
}

// Required lookup (throws if not found)
var requiredMember = mtCustomer.FindRequiredMember("Email");
Console.WriteLine($"Email type: {requiredMember.MemberType.Name}");
```

### Expression-based Member Finding (Preferred)

```csharp
public IMetaTypeMember? FindMember<TProperty>(Expression<Func<Customer, TProperty>> expression)
public IMetaTypeMember FindRequiredMember<TProperty>(Expression<Func<Customer, TProperty>> expression)
```

**FindMember&lt;TProperty&gt;(expression)**
- Type-safe member lookup using lambda expressions
- Supports IntelliSense and refactoring
- Returns `null` if the expression doesn't resolve to a property

**FindRequiredMember&lt;TProperty&gt;(expression)**
- Type-safe required member lookup
- Throws `InvalidOperationException` if not found
- Throws `ArgumentException` if expression is not a member access

**Example:**
```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

// Type-safe lookup (preferred)
var nameMember = mtCustomer.FindMember(c => c.Name);
var emailMember = mtCustomer.FindMember(c => c.Email);

// Required lookup with expression
var idMember = mtCustomer.FindRequiredMember(c => c.Id);

// Type inference works automatically
var addressesMember = mtCustomer.FindMember(c => c.Addresses); // No need for <List<Address>>
```

## Interfaces

### IMetaType Interface

```csharp
public interface IMetaType
{
    Type ManagedType { get; }
    string ManagedTypeName { get; }
    string ManagedTypeNamespace { get; }
    string ManagedTypeAssembly { get; }
    string ManagedTypeFullName { get; }
    Type[]? GenericTypeArguments { get; }
    Attribute[]? Attributes { get; }
    IReadOnlyList<IMetaTypeMember> Members { get; }
    
    IMetaTypeMember? FindMember(string name);
    IMetaTypeMember FindRequiredMember(string name);
}
```

### IMetaType&lt;T&gt; Interface

```csharp
public interface IMetaType<T> : IMetaType
{
    IMetaTypeMember? FindMember<TProperty>(Expression<Func<T, TProperty>> expression);
    IMetaTypeMember FindRequiredMember<TProperty>(Expression<Func<T, TProperty>> expression);
}
```

The generic interface provides strongly-typed expression-based member finding.

## Usage Patterns

### Iterating Members

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

foreach (var member in mtCustomer.Members)
{
    Console.WriteLine($"{member.MemberName}:");
    Console.WriteLine($"  Type: {member.MemberType.Name}");
    Console.WriteLine($"  HasSetter: {member.HasSetter}");
    Console.WriteLine($"  IsList: {member.IsList}");
    
    if (member.IsMetaType)
    {
        Console.WriteLine($"  Cross-reference: {member.MetaType?.ManagedTypeName}");
    }
}
```

### Type Information

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

Console.WriteLine($"Type Information:");
Console.WriteLine($"  Name: {mtCustomer.ManagedTypeName}");
Console.WriteLine($"  Namespace: {mtCustomer.ManagedTypeNamespace}");
Console.WriteLine($"  Assembly: {mtCustomer.ManagedTypeAssembly}");
Console.WriteLine($"  Full Name: {mtCustomer.ManagedTypeFullName}");

if (mtCustomer.GenericTypeArguments != null)
{
    Console.WriteLine($"  Generic Arguments: {string.Join(", ", mtCustomer.GenericTypeArguments.Select(t => t.Name))}");
}
```

### Attribute Access

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();

if (mtCustomer.Attributes != null)
{
    Console.WriteLine("Type Attributes:");
    foreach (var attr in mtCustomer.Attributes)
    {
        Console.WriteLine($"  {attr.GetType().Name}");
    }
}
```

## Performance Considerations

### Singleton Pattern
- Each MetaType uses a thread-safe singleton pattern
- Zero allocation after first access
- Lazy initialization ensures optimal startup performance

### Compile-time Constants
- All metadata is generated as compile-time constants
- No runtime reflection or expensive operations
- Expression compilation happens once and is cached

### Memory Efficiency
- Members collection is a compile-time array
- Attributes array is only created if attributes exist
- Generic type arguments array is only created for generic types

## Thread Safety

All MetaType classes are fully thread-safe:

- Singleton instance creation uses lazy initialization
- All properties return immutable data
- Members collection is read-only
- No mutable state after initialization

## EfCore Integration

When EfCore integration is enabled, MetaType classes also implement `IMetaTypeEfCore`:

```csharp
public partial class CustomerMetaType : IMetaType, IMetaType<Customer>, IMetaTypeEfCore
{
    // EfCore-specific properties
    public string? TableName => "Customers";
    public IReadOnlyList<IMetaTypeMemberEfCore> Keys => [
        CustomerMetaTypeMemberId.Instance
    ];
}
```

See [EfCore Extensions](./EfCoreExtensions.md) for detailed information about EfCore-specific features.