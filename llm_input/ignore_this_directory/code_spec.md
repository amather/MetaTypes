# This file

This file provides the specification for LLMs to create and work on a .NET library project.


# MetaTypes Library Specification - Instance Pattern

## Overview

The MetaTypes library provides a source generator that creates compile-time metadata for .NET classes, offering both static compile-time access and runtime polymorphic capabilities through a singleton instance pattern.

## Core Interfaces and Models

### IMetaType Interface

```csharp
using System;
using System.Collections.Generic;

namespace MetaTypes.Core.Interfaces;

public interface IMetaType
{
    // Static abstract members for compile-time access
    static abstract string TypeName { get; }
    static abstract string FullTypeName { get; }
    static abstract string Namespace { get; }
    static abstract Type RuntimeType { get; }
    static abstract IReadOnlyList<IMetaTypeMember> Members { get; }
    static abstract IReadOnlyList<Attribute> Attributes { get; }
    
    // Static abstract methods
    static abstract IMetaTypeMember? GetMember(string name);
    static abstract IReadOnlyList<IMetaTypeMember> GetMembersOfType<T>();
    static abstract IReadOnlyList<IMetaTypeMember> GetMembersWithAttribute<T>() where T : Attribute;
    
    // Instance members for runtime polymorphism
    string GetTypeName();
    string GetFullTypeName();
    string GetNamespace();
    Type GetRuntimeType();
    IReadOnlyList<IMetaTypeMember> GetMembers();
    IReadOnlyList<Attribute> GetAttributes();
    
    // Instance methods
    IMetaTypeMember? GetMember(string name);
    IReadOnlyList<IMetaTypeMember> GetMembersOfType<T>();
    IReadOnlyList<IMetaTypeMember> GetMembersWithAttribute<T>() where T : Attribute;
}
```

### IMetaTypeMember Interface

```csharp
using System;
using System.Collections.Generic;

namespace MetaTypes.Core.Interfaces;

public interface IMetaTypeMember
{
    // Static abstract members for compile-time access
    static abstract string PropertyName { get; }
    static abstract Type PropertyType { get; }
    static abstract string PropertyTypeName { get; }
    static abstract bool IsEnum { get; }
    static abstract bool IsNullable { get; }
    static abstract bool IsList { get; }
    static abstract bool IsGeneric { get; }
    static abstract bool IsMetaType { get; }
    static abstract IReadOnlyList<Attribute> Attributes { get; }
    static abstract Type[]? GenericArguments { get; }
    static abstract IMetaType? PropertyMetaType { get; }
    
    // Static abstract methods
    static abstract T? GetAttribute<T>() where T : Attribute;
    static abstract IReadOnlyList<T> GetAttributes<T>() where T : Attribute;
    
    // Instance members for runtime polymorphism
    string GetPropertyName();
    Type GetPropertyType();
    string GetPropertyTypeName();
    bool GetIsEnum();
    bool GetIsNullable();
    bool GetIsList();
    bool GetIsGeneric();
    bool GetIsMetaType();
    IReadOnlyList<Attribute> GetAttributes();
    Type[]? GetGenericArguments();
    IMetaType? GetPropertyMetaType();
    
    // Instance methods
    T? GetAttribute<T>() where T : Attribute;
    IReadOnlyList<T> GetAttributes<T>() where T : Attribute;
}
```

## Generated Code Structure

### Generated MetaType Class Template

For each class marked with `[MetaType]`, generate:

```csharp
// Generated for: public class User { public string Name { get; set; } public int Age { get; set; } }

using System;
using System.Collections.Generic;
using System.Linq;
using MetaTypes.Core.Interfaces;

namespace MyProject.MetaTypes;

public sealed class UserMetaType : IMetaType
{
    // Singleton instance
    public static readonly UserMetaType Instance = new();
    private UserMetaType() { }
    
    // Static abstract implementation (compile-time access)
    public static string TypeName => "User";
    public static string FullTypeName => "MyNamespace.User";
    public static string Namespace => "MyNamespace";
    public static Type RuntimeType => typeof(MyNamespace.User);
    
    private static readonly IReadOnlyList<IMetaTypeMember> _members =
    [
        UserMetaType_Name.Instance,
        UserMetaType_Age.Instance
    ];
    
    public static IReadOnlyList<IMetaTypeMember> Members => _members;
    
    private static readonly IReadOnlyList<Attribute> _attributes =
    [
        // Include attributes from the original class
    ];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    
    // Static abstract methods implementation
    public static IMetaTypeMember? GetMember(string name) => name switch
    {
        "Name" => UserMetaType_Name.Instance,
        "Age" => UserMetaType_Age.Instance,
        _ => null
    };
    
    public static IReadOnlyList<IMetaTypeMember> GetMembersOfType<T>() =>
        Members.Where(m => typeof(T).IsAssignableFrom(m.GetPropertyType())).ToList();
    
    public static IReadOnlyList<IMetaTypeMember> GetMembersWithAttribute<T>() where T : Attribute =>
        Members.Where(m => m.GetAttribute<T>() is not null).ToList();
    
    // Instance implementation (runtime polymorphism)
    public string GetTypeName() => TypeName;
    public string GetFullTypeName() => FullTypeName;
    public string GetNamespace() => Namespace;
    public Type GetRuntimeType() => RuntimeType;
    public IReadOnlyList<IMetaTypeMember> GetMembers() => Members;
    public IReadOnlyList<Attribute> GetAttributes() => Attributes;
    
    // Instance methods - forward to static implementations
    IMetaTypeMember? IMetaType.GetMember(string name) => GetMember(name);
    
    IReadOnlyList<IMetaTypeMember> IMetaType.GetMembersOfType<T>() =>
        GetMembersOfType<T>();
    
    IReadOnlyList<IMetaTypeMember> IMetaType.GetMembersWithAttribute<T>() =>
        GetMembersWithAttribute<T>();
}
```

### Generated MetaTypeMember Class Template

For each property/field:

```csharp
// Generated for: public string Name { get; set; }

using System;
using System.Collections.Generic;
using System.Linq;
using MetaTypes.Core.Interfaces;

namespace MyProject.MetaTypes;

public sealed class UserMetaType_Name : IMetaTypeMember
{
    // Singleton instance
    public static readonly UserMetaType_Name Instance = new();
    private UserMetaType_Name() { }
    
    // Static abstract implementation (compile-time access)
    public static string PropertyName => "Name";
    public static Type PropertyType => typeof(string);
    public static string PropertyTypeName => "string";
    public static bool IsEnum => false;
    public static bool IsNullable => false;
    public static bool IsList => false;
    public static bool IsGeneric => false;
    public static bool IsMetaType => false;
    
    private static readonly IReadOnlyList<Attribute> _attributes = [];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => null;
    public static IMetaType? PropertyMetaType => null;
    
    // Static abstract methods implementation
    public static T? GetAttribute<T>() where T : Attribute =>
        Attributes.OfType<T>().FirstOrDefault();
    
    public static IReadOnlyList<T> GetAttributes<T>() where T : Attribute =>
        Attributes.OfType<T>().ToList();
    
    // Instance implementation (runtime polymorphism)
    public string GetPropertyName() => PropertyName;
    public Type GetPropertyType() => PropertyType;
    public string GetPropertyTypeName() => PropertyTypeName;
    public bool GetIsEnum() => IsEnum;
    public bool GetIsNullable() => IsNullable;
    public bool GetIsList() => IsList;
    public bool GetIsGeneric() => IsGeneric;
    public bool GetIsMetaType() => IsMetaType;
    public IReadOnlyList<Attribute> GetAttributes() => Attributes;
    public Type[]? GetGenericArguments() => GenericArguments;
    public IMetaType? GetPropertyMetaType() => PropertyMetaType;
    
    // Instance methods - forward to static implementations
    T? IMetaTypeMember.GetAttribute<T>() => GetAttribute<T>();
    
    IReadOnlyList<T> IMetaTypeMember.GetAttributes<T>() => GetAttributes<T>();
}
```

### MetaType Cross-Reference Examples

#### Simple MetaType Reference

```csharp
// Generated for: public User? Clerk { get; set; } in SalesOrder class

using System;
using System.Collections.Generic;
using System.Linq;
using MetaTypes.Core.Interfaces;

namespace MyProject.MetaTypes;

public sealed class SalesOrderMetaType_Clerk : IMetaTypeMember
{
    public static readonly SalesOrderMetaType_Clerk Instance = new();
    private SalesOrderMetaType_Clerk() { }
    
    public static string PropertyName => "Clerk";
    public static Type PropertyType => typeof(User);
    public static string PropertyTypeName => "User?";
    public static bool IsEnum => false;
    public static bool IsNullable => true;
    public static bool IsList => false;
    public static bool IsGeneric => false;
    public static bool IsMetaType => true;
    
    private static readonly IReadOnlyList<Attribute> _attributes = [];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => null;
    
    // Direct reference to UserMetaType instance
    public static IMetaType? PropertyMetaType => UserMetaType.Instance;
    
    // Static abstract methods implementation
    public static T? GetAttribute<T>() where T : Attribute =>
        Attributes.OfType<T>().FirstOrDefault();
    
    public static IReadOnlyList<T> GetAttributes<T>() where T : Attribute =>
        Attributes.OfType<T>().ToList();
    
    // Instance implementations
    public string GetPropertyName() => PropertyName;
    public Type GetPropertyType() => PropertyType;
    public string GetPropertyTypeName() => PropertyTypeName;
    public bool GetIsEnum() => IsEnum;
    public bool GetIsNullable() => IsNullable;
    public bool GetIsList() => IsList;
    public bool GetIsGeneric() => IsGeneric;
    public bool GetIsMetaType() => IsMetaType;
    public IReadOnlyList<Attribute> GetAttributes() => Attributes;
    public Type[]? GetGenericArguments() => GenericArguments;
    public IMetaType? GetPropertyMetaType() => PropertyMetaType;
    
    // Instance methods - forward to static implementations
    T? IMetaTypeMember.GetAttribute<T>() => GetAttribute<T>();
    
    IReadOnlyList<T> IMetaTypeMember.GetAttributes<T>() => GetAttributes<T>();
}
```

#### Generic Collection with MetaType Element

```csharp
// Generated for: public List<OrderItem> Items { get; set; } where OrderItem has [MetaType]

using System;
using System.Collections.Generic;
using System.Linq;
using MetaTypes.Core.Interfaces;

namespace MyProject.MetaTypes;

public sealed class SalesOrderMetaType_Items : IMetaTypeMember
{
    public static readonly SalesOrderMetaType_Items Instance = new();
    private SalesOrderMetaType_Items() { }
    
    public static string PropertyName => "Items";
    public static Type PropertyType => typeof(List<OrderItem>);
    public static string PropertyTypeName => "List<OrderItem>";
    public static bool IsEnum => false;
    public static bool IsNullable => false;
    public static bool IsList => true;
    public static bool IsGeneric => true;
    public static bool IsMetaType => true; // Generic argument is a MetaType
    
    private static readonly IReadOnlyList<Attribute> _attributes = [];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => [typeof(OrderItem)];
    
    // Reference to OrderItemMetaType instance (for the generic element type, not the collection)
    public static IMetaType? PropertyMetaType => OrderItemMetaType.Instance;
    
    // Static abstract methods implementation
    public static T? GetAttribute<T>() where T : Attribute =>
        Attributes.OfType<T>().FirstOrDefault();
    
    public static IReadOnlyList<T> GetAttributes<T>() where T : Attribute =>
        Attributes.OfType<T>().ToList();
    
    // Instance implementations
    public string GetPropertyName() => PropertyName;
    public Type GetPropertyType() => PropertyType;
    public string GetPropertyTypeName() => PropertyTypeName;
    public bool GetIsEnum() => IsEnum;
    public bool GetIsNullable() => IsNullable;
    public bool GetIsList() => IsList;
    public bool GetIsGeneric() => IsGeneric;
    public bool GetIsMetaType() => IsMetaType;
    public IReadOnlyList<Attribute> GetAttributes() => Attributes;
    public Type[]? GetGenericArguments() => GenericArguments;
    public IMetaType? GetPropertyMetaType() => PropertyMetaType;
    
    // Instance methods - forward to static implementations
    T? IMetaTypeMember.GetAttribute<T>() => GetAttribute<T>();
    
    IReadOnlyList<T> IMetaTypeMember.GetAttributes<T>() => GetAttributes<T>();
}
```

## Usage Examples

### Compile-Time Usage (Static Access)

```csharp
// Direct static access - most efficient for known types
var userName = UserMetaType.TypeName;           // "User"
var userMembers = UserMetaType.Members;         // Collection of IMetaTypeMember
var nameProperty = UserMetaType_Name.PropertyName; // "Name"

// Type-safe member access
var member = UserMetaType.GetMember("Name");    // UserMetaType_Name.Instance
if (member is not null)
{
    Console.WriteLine($"Property: {member.GetPropertyName()} ({member.GetPropertyTypeName()})");
}

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

### Runtime Usage (Polymorphic Access)

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

// Collection of MetaTypes
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

### Generic Helper Methods

```csharp
// Generic helper that works with any MetaType at compile-time
public static void PrintMetaTypeInfo<T>() where T : IMetaType
{
    Console.WriteLine($"Type: {T.TypeName}");
    Console.WriteLine($"Namespace: {T.Namespace}");
    Console.WriteLine($"Member count: {T.Members.Count}");
}

// Usage
PrintMetaTypeInfo<UserMetaType>();      // Compile-time resolved
PrintMetaTypeInfo<SalesOrderMetaType>(); // Compile-time resolved

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

### Attribute-Based Filtering

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

## Benefits of the Instance Pattern

### 1. **Dual Access Patterns**
- **Compile-time**: Static access for maximum performance when types are known
- **Runtime**: Instance access for polymorphic scenarios and dynamic type handling

### 2. **Interface Implementation**
- Non-static classes can properly implement interfaces
- Enables polymorphic collections and generic constraints
- Maintains type safety while allowing runtime flexibility

### 3. **Cross-Reference Resolution**
- `PropertyMetaType` can directly reference other MetaType instances
- Enables navigation between related MetaTypes
- Supports complex object graph analysis

### 4. **Memory Efficiency**
- Singleton pattern ensures only one instance per type
- Static data is shared across all access patterns
- Minimal memory overhead (~0.5 MB for 200 classes × 20 properties)

### 5. **Performance Characteristics**
- Static access: Zero overhead, compile-time resolved
- Instance access: Single indirection, faster than reflection
- No runtime type analysis or expensive lookups

## Implementation Guidelines

### Source Generator Requirements

1. **Namespace Resolution**: Generate MetaTypes in a consistent namespace (e.g., `{ProjectName}.MetaTypes`)
2. **Cross-Reference Detection**: Identify when properties reference other MetaType-decorated classes
3. **Generic Handling**: Properly analyze generic collections and their element types
4. **Attribute Preservation**: Include all attributes from source properties in generated metadata
5. **Nullable Reference Type Support**: Correctly detect and represent nullable reference types

### Code Generation Best Practices

1. **Include all necessary using statements** in generated files
2. **Use explicit interface implementation** for instance methods to avoid naming conflicts
3. **Generate defensive null checks** where appropriate
4. **Maintain consistent naming conventions** across all generated types
5. **Include XML documentation comments** for better developer experience

### Error Handling

1. **Validate MetaType attribute targets** (classes/records only)
2. **Handle circular references** gracefully
3. **Provide clear diagnostic messages** for unsupported scenarios
4. **Generate partial classes** to allow manual extensions if needed

This specification provides a complete foundation for implementing a MetaTypes library that combines compile-time performance with runtime flexibility through the singleton instance pattern.