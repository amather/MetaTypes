
# MetaTypes Source Generator

Parses classes, structs, records and enums and generates a corresponding MetaType class as a helper to reduce reflection overhead during runtime.
Each class gets its corresponding IMetaType class (sealed, accessed by .Instance property) that reveals a IReadOnlyList<IMetaTypeMember> list for each property of the class. The member, which is also a sealed class accessed by static .Instance property provides several information about the member.

The source generator runs on a per-assembly basis and needs to extract the common namespace across all identified types (identified by [MetaType] attribute). Alternatively, a config option for the generator <MetaTypeAssemblyName /> can be used to overwrite this assembly-level name. On the assembly level, the generator creates a discovery class, `MetaTypes`. For example:

Assembly: `MyApp.Business`
Types: `MyApp.Business.Models.Customer`, `MyApp.Business.Dto.CustomerResponse`
Config options: none
Generated type: `MyApp.Business.MetaTypes`  (common part: 'MyApp.Business', discovery type: 'MetaTypes')

Assembly: `MyApp.Auth`
Types: `MyApp.Auth.Models.PasswordUser`, `MyApp.Auth.Dto.LoginResponse`
Config options: <MetaTypeAssemblyName>MyAppAuth</MetaTypeAssemblyName>
Generated type: `MyAppAuth.MetaTypes` 

The generator also supports DI-extension methods, for example, in `Program.cs` a user may write:

```
services.AddMetaTypes<MyApp.Business.MetaTypes>();
services.AddMetaTypes<MyAppAuth.MetaTypes>();
```

The extension method clearly has to come from a shared library, MetaTypes.Abstractions, MetaType consumers reference, ie is not generated.  But the `MetaTypes` type is generated and contains the information (e.g. a IReadOnlyList<IMetaType>) required to add the metatypes to DI.

The clue is that consumer can request the MetaType for a specific managed type, e.g.:
* `serviceProvider.GetRequiredService<IMetaType<Customer>>()`
* `serviceProvider.GetMetaType<Customer>()`
* `serviceProvider.GetMetaType(Type t)`

## IMetaTypeProvider

To reduce the amount of reflection used, here's a suggestion:
The .AddMetaTypes<>() methods may once check for the generic type argument being of type IMetaTypeProvider. If so, it accesses it's instance property,
`.Instance` which then resolves to a IMetaTypeProvider instance that returns the IReadOnlyList<> of assembly IMetaTypes. For each of these meta types, it adds the IMetaType<ManagedType>, e.g. IMetaType<Customer>, type to DI.
`IServiceProvider.GetMetaType<>()` and `IServiceProvider.GetMetaType(Type t)` should be shortcuts to `IServiceProvider.GetRequiredService(IMetaType<>)`.



## IMetaType

As for the IMetaType itself, it should reveal, at least these properties:

* `Type ManagedType` -> .NET type, e.g. `typeof(Customer)`
* `string ManagedTypeName` -> e.g. `Customer`
* `string ManagedTypeNamespace` -> e.g. `MyApp.Business.Models`
* `string ManagedTypeAssembly` -> e.g. `MyApp.Business`
* `string ManagedTypeFullName` -> e.g. `MyApp.Business.Models.Customer`
* `bool IsNullable` -> must work for reference types and structs/primitives, ie Nullable<T>
* `Type[]? GenericTypeArguments` -> null if not a generic type, else the Type of the generic type arguments
* `Attribute[]? Attributes` -> null if none, else the attributes classes with their initialized properties
* `IReadOnlyList<IMetaTypeMember> Members` -> the members meta types for the class properties

Additionally, these methods must be provided:

* `IMetaTypeMember? FindMember(string s)` -> e.g. 'FindMember("Name")'
* `IMetaTypeMember FindRequiredMember(string s)` -> throwing variant
* `IMetaTypeMember? FindMember<T>(Expression<Func<T, object?>>)` -> e.g. 'FindMember<Customer>(x => x.Name)'
* `IMetaTypeMember FindRequiredMember<T>(Expression<Func<T, object?>>)` -> throwing variant

## IMetaTypeMember

As for IMetaTypeMember, it should have at least the following properties:

* `string MemberName` -> e.g. "Name", "Age", etc.
* `Type MemberType` -> e.g. "typeof(string)", "typeof(List<CustomerAddress>)", etc.
* `bool IsMetaType` -> returns true if there's a corresponding IMetaType for MemberType (Customer -> IMetaType<Customer>: true, string Name: false)
* `IMetaType? MetaType` -> return the IMetaType if IsMetaType
* `bool HasSetter` -> can be written (property has setter)
* `bool IsList` -> if IList<>
* `Type[]? GenericTypeArguments` -> if generic type, the generic type arguments
* `Attribute[]? Attributes` -> null if property has no attributes, else an array of the attributes with their values

# General approach

A suggestion:

```
namespace MyApp.Business;
public sealed class MetaTypes : IMetaTypeProvider
{
  private static MetaTypes? _instance;
  public static MetaTypes Instance => _instance ??= new();

  public IReadOnlyList<IMetaType> AssemblyMetaTypes => [
    CustomerMetaType.Instance,
    SalesOrderMetaType.Instance,
  ];

}
```

```
public sealed class CustomerMetaType : IMetaType, IMetaType<Customer>
{
  private static CustomerMetaType? _instance;
  public static CustomerMetaType Instance => _instance ??= new();

  public IReadOnlyList<IMetaTypeMember> Members => [
    CustomerMetaTypeMemberName.Instance,
    CustomerMetaTypeMemberAddress.Instance,
    CustomerMetaTypeMemberIsFlagged.Instance,
  ];
}
```

```
public sealed class CustomerMetaTypeMemberName : IMetaTypeMember
{
  private static CustomerMetaTypeMemberName? _instance;
  public static CustomerMetaTypeMemberName Instance => _instance ??= new();

  public string MemberName => "Name";
  public Type MemberType => typeof(string);
  public bool IsMetaType => false;
  public IMetaType? MetaType => null;
  public bool HasSetter => true;
  public bool IsList => false;
  public Type[]? GenericTypeArguments => null;
  public Attribute[]? Attributes => null;

}
```

It's important that the values of IMetaType and IMetaTypeMember instance properties are compile-time constants, ie. they can be defined at compile time
and will never change at runtime. Every optimization for the compiler (IReadOnly<> types, probably Span<>, etc. should be used to accommodate for that fact.).

# Samples

`Sample` directory provides a few types that may serve as a reference for implementation. But any real Sample project, should have at least two model assemblies and one executing assembly, so we can test multi-assembly source generation.
