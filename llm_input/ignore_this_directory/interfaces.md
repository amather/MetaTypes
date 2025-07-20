# Interface Definitions and Contracts

## IMetaType Interface

The core interface that all generated MetaType classes implement, providing both static compile-time access and runtime polymorphic capabilities.

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

## IMetaTypeMember Interface

Represents metadata for individual properties and fields within a MetaType.

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

## Design Principles

### Dual Access Pattern
- **Static access**: Maximum performance for compile-time known types
- **Instance access**: Runtime polymorphism for dynamic scenarios

### Cross-Reference Support
- `PropertyMetaType` enables navigation between related MetaTypes
- Supports complex object graph analysis
- Handles both direct references and generic collections

### Memory Efficiency
- Singleton pattern ensures one instance per type
- Static data shared across access patterns
- Minimal runtime overhead

### Type Safety
- Generic methods maintain compile-time type checking
- Nullable annotations for modern C# compatibility
- Explicit interface implementation prevents naming conflicts