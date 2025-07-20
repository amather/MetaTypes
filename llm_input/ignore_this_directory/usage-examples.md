# Usage Patterns and Examples

## Compile-Time Usage (Static Access)

### Direct Static Access
Most efficient for known types at compile time:

```csharp
// Direct static access - zero overhead
var userName = UserMetaType.TypeName;           // "User"
var userMembers = UserMetaType.Members;         // Collection of IMetaTypeMember
var nameProperty = UserMetaType_Name.PropertyName; // "Name"

// Type-safe member access
var member = UserMetaType.GetMember("Name");    // UserMetaType_Name.Instance
if (member is not null)
{
    Console.WriteLine($"Property: {member.GetPropertyName()} ({member.GetPropertyTypeName()})");
}
```

### MetaType Cross-References
Navigate between related MetaTypes:

```csharp
// MetaType cross-references
if (SalesOrderMetaType_Clerk.IsMetaType)
{
    var userMetaType = SalesOrderMetaType_Clerk.PropertyMetaType; // UserMetaType.Instance
    Console.WriteLine($"Clerk property references: {userMetaType?.GetTypeName()}");
    
    // Access members of the referenced MetaType
    foreach (var member in userMetaType?.GetMembers() ?? [])
    {
        Console.WriteLine($"  - {member.GetPropertyName()}: {member.GetPropertyTypeName()}");
    }
}
```

## Runtime Usage (Polymorphic Access)

### Working with MetaTypes Polymorphically

```csharp
// Work with MetaTypes polymorphically
public void AnalyzeMetaType(IMetaType metaType)
{
    Console.WriteLine($"Analyzing type: {metaType.GetTypeName()}");
    Console.WriteLine($"Namespace: {metaType.GetNamespace()}");
    Console.WriteLine($"Member count: {metaType.GetMembers().Count}");
    
    foreach (var member in metaType.GetMembers())
    {
        Console.WriteLine($"  {member.GetPropertyName()}: {member.GetPropertyTypeName()}");
        
        if (member.GetIsMetaType() && member.GetPropertyMetaType() is IMetaType referencedType)
        {
            Console.WriteLine($"    -> References: {referencedType.GetTypeName()}");
        }
    }
}

// Usage
AnalyzeMetaType(UserMetaType.Instance);
AnalyzeMetaType(SalesOrderMetaType.Instance);
```

### Collections of MetaTypes

```csharp
// Collection of MetaTypes for runtime processing
var allMetaTypes = new List<IMetaType>
{
    UserMetaType.Instance,
    SalesOrderMetaType.Instance,
    OrderItemMetaType.Instance
};

foreach (var metaType in allMetaTypes)
{
    AnalyzeMetaType(metaType);
}
```

## Generic Helper Methods

### Compile-Time Generic Helpers

```csharp
// Generic helper that works with any MetaType at compile-time
public static void PrintMetaTypeInfo<T>() where T : IMetaType
{
    Console.WriteLine($"Type: {T.TypeName}");
    Console.WriteLine($"Namespace: {T.Namespace}");
    Console.WriteLine($"Member count: {T.Members.Count}");
}

// Usage - fully resolved at compile time
PrintMetaTypeInfo<UserMetaType>();      
PrintMetaTypeInfo<SalesOrderMetaType>(); 

// Generic helper for MetaType members
public static void PrintMemberInfo<T>() where T : IMetaTypeMember
{
    Console.WriteLine($"Property: {T.PropertyName} ({T.PropertyTypeName})");
    Console.WriteLine($"Is MetaType: {T.IsMetaType}");
    
    if (T.PropertyMetaType is IMetaType metaType)
    {
        Console.WriteLine($"  References: {metaType.GetTypeName()}");
    }
}

// Usage
PrintMemberInfo<UserMetaType_Name>();           // string property
PrintMemberInfo<SalesOrderMetaType_Clerk>();    // User? property
PrintMemberInfo<SalesOrderMetaType_Items>();    // List<OrderItem> property
```

## Attribute-Based Filtering

### Finding Properties by Attributes

```csharp
// Find all properties with specific attributes
var validationProperties = UserMetaType.GetMembersWithAttribute<RequiredAttribute>();
foreach (var prop in validationProperties)
{
    Console.WriteLine($"Required property: {prop.GetPropertyName()}");
}

// Find all string properties
var stringProperties = UserMetaType.GetMembersOfType<string>();
foreach (var prop in stringProperties)
{
    Console.WriteLine($"String property: {prop.GetPropertyName()}");
}

// Access specific attributes
var maxLengthAttr = UserMetaType_Name.GetAttribute<MaxLengthAttribute>();
if (maxLengthAttr is not null)
{
    Console.WriteLine($"Max length for Name: {maxLengthAttr.Length}");
}
```

## Advanced Scenarios

### Object Graph Analysis

```csharp
public void AnalyzeObjectGraph(IMetaType rootType, HashSet<string>? visited = null)
{
    visited ??= new HashSet<string>();
    
    if (!visited.Add(rootType.GetFullTypeName()))
    {
        Console.WriteLine($"Circular reference detected: {rootType.GetTypeName()}");
        return;
    }
    
    Console.WriteLine($"Analyzing: {rootType.GetTypeName()}");
    
    foreach (var member in rootType.GetMembers())
    {
        Console.WriteLine($"  {member.GetPropertyName()}: {member.GetPropertyTypeName()}");
        
        if (member.GetIsMetaType() && member.GetPropertyMetaType() is IMetaType referencedType)
        {
            Console.WriteLine($"    -> Navigating to: {referencedType.GetTypeName()}");
            AnalyzeObjectGraph(referencedType, visited);
        }
    }
    
    visited.Remove(rootType.GetFullTypeName());
}
```

### Validation Framework Integration

```csharp
public class MetaTypeValidator
{
    public ValidationResult ValidateObject<T>(T obj) where T : IMetaType
    {
        var results = new List<ValidationError>();
        
        foreach (var member in T.Members)
        {
            var value = GetPropertyValue(obj, member.GetPropertyName());
            
            // Required validation
            if (member.GetAttribute<RequiredAttribute>() is not null && value is null)
            {
                results.Add(new ValidationError($"{member.GetPropertyName()} is required"));
            }
            
            // String length validation
            if (member.GetAttribute<MaxLengthAttribute>() is MaxLengthAttribute maxLength && 
                value is string str && str.Length > maxLength.Length)
            {
                results.Add(new ValidationError($"{member.GetPropertyName()} exceeds max length {maxLength.Length}"));
            }
        }
        
        return new ValidationResult(results);
    }
}
```

### Performance Comparison

```csharp
// MetaTypes vs Reflection performance
[Benchmark]
public void ReflectionApproach()
{
    var type = typeof(User);
    var properties = type.GetProperties();
    foreach (var prop in properties)
    {
        var name = prop.Name;
        var propType = prop.PropertyType;
        var attributes = prop.GetCustomAttributes().ToArray();
    }
}

[Benchmark]
public void MetaTypesApproach()
{
    var members = UserMetaType.Members;
    foreach (var member in members)
    {
        var name = member.GetPropertyName();
        var propType = member.GetPropertyType();
        var attributes = member.GetAttributes();
    }
}
```

## Best Practices

### Prefer Static Access When Possible
- Use static access for known types at compile time
- Reserve instance access for polymorphic scenarios

### Leverage Cross-References
- Use `PropertyMetaType` for navigation between related types
- Check `IsMetaType` before accessing `PropertyMetaType`

### Combine with Generic Constraints
- Use MetaType interfaces as generic constraints
- Enable compile-time type safety with runtime flexibility

### Attribute Caching
- Cache attribute lookups for frequently accessed members
- Use static readonly fields for expensive attribute queries