# Generated Code Templates and Examples

## MetaType Class Template

For each class marked with `[MetaType]`, the source generator creates a corresponding MetaType class:

### Basic MetaType Example

```csharp
// Source: public class User { public string Name { get; set; } public int Age { get; set; } }

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

## MetaTypeMember Class Templates

### Simple Property Member

```csharp
// Source: public string Name { get; set; }

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

### MetaType Reference Property

```csharp
// Source: public User? Clerk { get; set; } in SalesOrder class

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
    
    // Implementation methods...
}
```

### Generic Collection with MetaType Elements

```csharp
// Source: public List<OrderItem> Items { get; set; } where OrderItem has [MetaType]

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
    
    // Reference to OrderItemMetaType instance (for the generic element type)
    public static IMetaType? PropertyMetaType => OrderItemMetaType.Instance;
    
    // Implementation methods...
}
```

## Generation Rules

### Naming Convention
- MetaType classes: `{TypeName}MetaType`
- Member classes: `{TypeName}MetaType_{PropertyName}`

### Namespace Strategy
- Analyze all `[MetaType]` attributed types in the project
- Find common namespace prefix
- Generate all MetaTypes in `{CommonNamespace}.MetaTypes` namespace
- Override with `MetaTypesNamespace` MSBuild property if specified

### Accessibility
- All generated classes are `public sealed`
- Singleton pattern with private constructors
- Static abstract interface implementation

### Type Analysis
- `IsMetaType = true` when `PropertyMetaType != null`
- `IsList = true` for collection types (List<T>, IEnumerable<T>, etc.)
- `IsGeneric = true` for generic types
- `IsNullable = true` for nullable reference/value types
- `GenericArguments` contains all generic type arguments
- `PropertyMetaType` points to the relevant MetaType (element type for collections)