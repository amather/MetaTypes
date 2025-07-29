# MetaTypeMember Classes

MetaTypeMember classes provide detailed metadata about individual properties of your types, including dynamic property access through `GetValue` and `SetValue` methods. For each public instance property, the generator creates a corresponding MetaTypeMember class.

## Class Structure

### Generated Class Pattern

For a property named `Name` on a `Customer` type, the generator creates:

```csharp
public partial class CustomerMetaTypeMemberName : IMetaTypeMember
{
    // Singleton pattern
    private static CustomerMetaTypeMemberName? _instance;
    public static CustomerMetaTypeMemberName Instance => _instance ??= new();
    
    // Property metadata
    public string MemberName => "Name";
    public Type MemberType => typeof(string);
    public bool HasSetter => true;
    public bool IsList => false;
    public Type[]? GenericTypeArguments => null;
    
    // Property attributes
    public Attribute[]? Attributes => [
        new RequiredAttribute(),
        new MaxLengthAttribute(100),
        // ... other attributes
    ];
    
    // Cross-reference detection
    public bool IsMetaType => false;
    public IMetaType? MetaType => null;
    
    // Dynamic property access
    public object? GetValue(object obj) { /* ... */ }
    public void SetValue(object obj, object? value) { /* ... */ }
}
```

## Properties

### Basic Property Information

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `MemberName` | `string` | Name of the property | `"Name"`, `"Email"`, `"Id"` |
| `MemberType` | `Type` | The `Type` of the property | `typeof(string)`, `typeof(int)` |
| `HasSetter` | `bool` | Whether the property has a setter | `true` for `{ get; set; }`, `false` for `{ get; }` |

### Collection Detection

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `IsList` | `bool` | Whether the property is a collection type | `true` for `List<T>`, `ICollection<T>`, etc. |
| `GenericTypeArguments` | `Type[]?` | Generic type arguments for collections | `[typeof(CustomerAddress)]` for `List<CustomerAddress>` |

**Supported Collection Types:**
- `List<T>`
- `IList<T>`
- `ICollection<T>`
- `IEnumerable<T>`

**Collection Example:**
```csharp
public class Customer
{
    public List<CustomerAddress> Addresses { get; set; } = [];
}

// Generated member
public partial class CustomerMetaTypeMemberAddresses : IMetaTypeMember
{
    public bool IsList => true;
    public Type[]? GenericTypeArguments => [typeof(CustomerAddress)];
    public Type MemberType => typeof(List<CustomerAddress>);
}
```

### Attributes

| Property | Type | Description |
|----------|------|-------------|
| `Attributes` | `Attribute[]?` | Array of attributes applied to the property |

**Attribute Handling:**
- Only attributes with parameterless constructors are captured
- `MetaTypeAttribute` is automatically excluded
- Complex attributes with constructor parameters are skipped
- Returns `null` if no attributes are present

**Attribute Example:**
```csharp
public class Customer
{
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = "";
}

// Generated member
public partial class CustomerMetaTypeMemberEmail : IMetaTypeMember
{
    public Attribute[]? Attributes => [
        new RequiredAttribute(),
        new EmailAddressAttribute()
        // MaxLengthAttribute skipped (has constructor parameters)
    ];
}
```

### Cross-Reference Detection

| Property | Type | Description |
|----------|------|-------------|
| `IsMetaType` | `bool` | Whether the property type (or collection element type) has a corresponding MetaType |
| `MetaType` | `IMetaType?` | Reference to the MetaType instance if `IsMetaType` is true |

**Cross-Reference Logic:**
- For direct types: Checks if `CustomerAddress` has a `CustomerAddressMetaType`
- For collections: Checks if the element type has a MetaType
- For nullable types: Checks the underlying type
- Works across assemblies when both MetaTypes are registered

**Cross-Reference Example:**
```csharp
public class Customer
{
    public CustomerAddress? PrimaryAddress { get; set; }
    public List<CustomerAddress> Addresses { get; set; } = [];
}

// Generated members
public partial class CustomerMetaTypeMemberPrimaryAddress : IMetaTypeMember
{
    public bool IsMetaType => true;
    public IMetaType? MetaType => Sample.Business.CustomerAddressMetaType.Instance;
}

public partial class CustomerMetaTypeMemberAddresses : IMetaTypeMember
{
    public bool IsMetaType => true; // Detects List<CustomerAddress> element type
    public IMetaType? MetaType => Sample.Business.CustomerAddressMetaType.Instance;
}
```

## Dynamic Property Access Methods

### GetValue Method

```csharp
public object? GetValue(object obj)
```

**Description:**
- Retrieves the current value of the property from the specified object
- Type-safe: validates that the object is of the expected type
- Returns the actual property value, boxed if it's a value type

**Example:**
```csharp
var customer = new Customer { Id = 42, Name = "John Doe" };
var mtCustomer = serviceProvider.GetMetaType<Customer>();

var mtmId = mtCustomer.FindMember(c => c.Id);
var mtmName = mtCustomer.FindMember(c => c.Name);

var id = mtmId.GetValue(customer);     // Returns: 42 (boxed int)
var name = mtmName.GetValue(customer); // Returns: "John Doe"
```

**Error Handling:**
- Throws `ArgumentException` if the object is not of the expected type
- Returns `null` for null property values
- Handles nullable value types correctly

### SetValue Method

```csharp
public void SetValue(object obj, object? value)
```

**Description:**
- Sets the property value on the specified object
- Type-safe: validates object type and performs value casting
- Handles special cases like init-only properties and read-only properties

**Example:**
```csharp
var customer = new Customer { Id = 42, Name = "John Doe" };
var mtCustomer = serviceProvider.GetMetaType<Customer>();

var mtmId = mtCustomer.FindMember(c => c.Id);
var mtmName = mtCustomer.FindMember(c => c.Name);

mtmId.SetValue(customer, 99);              // Sets Id = 99
mtmName.SetValue(customer, "Jane Smith");  // Sets Name = "Jane Smith"

Console.WriteLine($"{customer.Id}, {customer.Name}"); // "99, Jane Smith"
```

**Special Property Handling:**

**Init-only Properties:**
```csharp
public class Customer
{
    public int Id { get; init; }  // Init-only property
    public string Name { get; set; } = "";
}

// Generated SetValue for Id property
public void SetValue(object obj, object? value)
{
    throw new InvalidOperationException("Property 'Id' is init-only and cannot be set after object initialization.");
}
```

**Read-only Properties:**
```csharp
public class Customer
{
    public string FullName { get; }  // Read-only property
}

// Generated SetValue for FullName property
public void SetValue(object obj, object? value)
{
    throw new InvalidOperationException("Property 'FullName' is read-only and cannot be set.");
}
```

**Error Handling:**
- Throws `ArgumentException` if the object is not of the expected type
- Throws `InvalidOperationException` for init-only properties
- Throws `InvalidOperationException` for read-only properties
- Performs automatic type casting and validation

## Interfaces

### IMetaTypeMember Interface

```csharp
public interface IMetaTypeMember
{
    string MemberName { get; }
    Type MemberType { get; }
    bool HasSetter { get; }
    bool IsList { get; }
    Type[]? GenericTypeArguments { get; }
    Attribute[]? Attributes { get; }
    bool IsMetaType { get; }
    IMetaType? MetaType { get; }
    
    object? GetValue(object obj);
    void SetValue(object obj, object? value);
}
```

## Usage Patterns

### Property Inspection

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();
var mtmEmail = mtCustomer.FindMember(c => c.Email);

Console.WriteLine($"Property: {mtmEmail.MemberName}");
Console.WriteLine($"Type: {mtmEmail.MemberType.Name}");
Console.WriteLine($"Has Setter: {mtmEmail.HasSetter}");
Console.WriteLine($"Is Collection: {mtmEmail.IsList}");

if (mtmEmail.Attributes != null)
{
    Console.WriteLine($"Attributes: {string.Join(", ", mtmEmail.Attributes.Select(a => a.GetType().Name))}");
}
```

### Dynamic Object Manipulation

```csharp
var customer = new Customer();
var mtCustomer = serviceProvider.GetMetaType<Customer>();

// Set properties dynamically
var properties = new Dictionary<string, object>
{
    ["Id"] = 42,
    ["Name"] = "John Doe",
    ["Email"] = "john@example.com"
};

foreach (var (propName, value) in properties)
{
    var member = mtCustomer.FindMember(propName);
    if (member?.HasSetter == true)
    {
        member.SetValue(customer, value);
    }
}

// Read properties dynamically
foreach (var member in mtCustomer.Members)
{
    var value = member.GetValue(customer);
    Console.WriteLine($"{member.MemberName}: {value}");
}
```

### Cross-Reference Navigation

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();
var mtmAddresses = mtCustomer.FindMember(c => c.Addresses);

if (mtmAddresses.IsMetaType && mtmAddresses.MetaType != null)
{
    Console.WriteLine($"Addresses collection contains: {mtmAddresses.MetaType.ManagedTypeName}");
    
    // Can access the referenced MetaType
    var addressMetaType = mtmAddresses.MetaType;
    Console.WriteLine($"Address properties: {addressMetaType.Members.Count}");
    
    foreach (var addressMember in addressMetaType.Members)
    {
        Console.WriteLine($"  {addressMember.MemberName} ({addressMember.MemberType.Name})");
    }
}
```

### Collection Property Handling

```csharp
var mtCustomer = serviceProvider.GetMetaType<Customer>();
var mtmAddresses = mtCustomer.FindMember(c => c.Addresses);

if (mtmAddresses.IsList)
{
    Console.WriteLine($"Collection Type: {mtmAddresses.MemberType.Name}");
    
    if (mtmAddresses.GenericTypeArguments != null)
    {
        var elementType = mtmAddresses.GenericTypeArguments[0];
        Console.WriteLine($"Element Type: {elementType.Name}");
    }
    
    // Access collection dynamically
    var customer = new Customer 
    { 
        Addresses = [new CustomerAddress { Street = "123 Main St" }] 
    };
    
    var addresses = mtmAddresses.GetValue(customer) as IEnumerable;
    Console.WriteLine($"Address Count: {addresses?.Cast<object>().Count()}");
}
```

## Performance Considerations

### Singleton Pattern
- Each MetaTypeMember uses thread-safe singleton pattern
- Zero allocation after first access
- Lazy initialization for optimal performance

### Dynamic Access Performance
- `GetValue`/`SetValue` use direct property access (not reflection)
- Type checking is performed once per call
- Boxing/unboxing only when necessary
- Significantly faster than reflection-based approaches

### Memory Efficiency
- Attributes array only created when attributes exist
- Generic type arguments array only for collection properties
- Minimal memory footprint per member

## Thread Safety

All MetaTypeMember classes are fully thread-safe:
- Singleton instance creation is thread-safe
- All properties return immutable data
- `GetValue` and `SetValue` methods are thread-safe
- No mutable state after initialization

## EfCore Integration

When EfCore integration is enabled, MetaTypeMember classes also implement `IMetaTypeMemberEfCore`:

```csharp
public partial class CustomerMetaTypeMemberId : IMetaTypeMember, IMetaTypeMemberEfCore
{
    // EfCore-specific properties
    public bool IsKey => true;
    public bool IsForeignKey => false;
    public bool IsNotMapped => false;
    public IMetaTypeMember? ForeignKeyMember => null;
}
```

See [EfCore Extensions](./EfCoreExtensions.md) for detailed information about EfCore-specific member features.