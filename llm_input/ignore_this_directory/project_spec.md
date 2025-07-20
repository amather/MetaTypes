# MetaTypes Library Specification

## Implementation TODO List

Use this checklist to track implementation progress. Mark completed items with `[x]`.

### Phase 1: Project Foundation
- [ ] Setup Project Structure
  - [ ] Create solution file `MetaTypes.sln`
  - [ ] Create folder structure as specified
  - [ ] Setup `global.json` with .NET 9 SDK requirement
  - [ ] Create `Directory.Build.props` with shared properties
  - [ ] Setup `.editorconfig` for consistent coding standards
  - [ ] Create `nuget.config` if needed

### Phase 2: Core Library Implementation
- [ ] MetaTypes.Core Project
  - [ ] Create `MetaTypes.Core.csproj` with .NET 9 target
  - [ ] Implement `MetaTypeAttribute` with modern C# features
  - [ ] Create `IMetaType` interface with nullable annotations
  - [ ] Create `IMetaTypeMember` interface with nullable annotations
  - [ ] Implement base `MetaType` and `MetaTypeMember` classes if needed
  - [ ] Add XML documentation to all public APIs

### Phase 3: Source Generator Implementation
- [ ] MetaTypes.Generator Project
  - [ ] Create `MetaTypes.Generator.csproj` with .NET Standard 2.0 target
  - [ ] Implement `MetaTypeSourceGenerator` using `IIncrementalGenerator`
  - [ ] Create `TypeAnalyzer` for detecting `[MetaType]` attributed types
  - [ ] Create `MemberAnalyzer` for property/field analysis
  - [ ] Implement configuration support for MSBuild properties
  - [ ] Implement namespace analysis and common namespace detection with override support
  - [ ] Implement code generation using modern string interpolation and raw strings
  - [ ] Generate classes in configured or project-specific `{CommonNamespace}.MetaTypes` namespace
  - [ ] Add diagnostics for invalid usage scenarios
  - [ ] Test generator with simple types and configuration options

### Phase 4: Basic Usage Example
- [ ] Create Basic Sample Project
  - [ ] Create `samples/BasicUsage/BasicUsage.csproj`
  - [ ] Define simple test types with `[MetaType]` attribute
  - [ ] Add MSBuild configuration properties for testing
  - [ ] Demonstrate basic MetaType usage in `Program.cs`
  - [ ] Test both automatic and custom namespace generation
  - [ ] Verify generated code compiles and runs correctly
  - [ ] Document usage patterns in comments

### Phase 5: Meta-Package Setup
- [ ] MetaTypes Meta-Package
  - [ ] Create `MetaTypes.csproj` meta-package
  - [ ] Reference both Core and Generator packages
  - [ ] Test package installation and usage
  - [ ] Verify analyzer integration works correctly

### Phase 6: Meta-Package Setup
- [ ] MetaTypes Meta-Package
  - [ ] Create `MetaTypes.csproj` meta-package
  - [ ] Reference both Core and Generator packages
  - [ ] Test package installation and usage
  - [ ] Verify analyzer integration works correctly

### Phase 7: Advanced Features
- [ ] Complex Type Support
  - [ ] Handle generic types and constraints
  - [ ] Support nullable reference types correctly
  - [ ] Implement MetaType cross-references with `PropertyMetaType`
  - [ ] Handle collection types (List<T>, IEnumerable<T>, etc.) with MetaType elements
  - [ ] Support enum types with proper classification
  - [ ] Implement `IsMetaType` as shorthand for `PropertyMetaType != null`
  - [ ] Handle nested generics like `Dictionary<string, User>`

### Phase 8: Advanced Usage Example
- [ ] Create Advanced Sample Project
  - [ ] Create `samples/AdvancedUsage/AdvancedUsage.csproj`
  - [ ] Demonstrate complex type scenarios
  - [ ] Show MetaType cross-references in action
  - [ ] Include performance comparison with reflection
  - [ ] Document advanced patterns and best practices

### Phase 9: Testing Infrastructure
- [ ] Unit Tests Project
  - [ ] Create `MetaTypes.Tests.csproj`
  - [ ] Write generator tests using `Microsoft.CodeAnalysis.Testing`
  - [ ] Test attribute detection and member analysis
  - [ ] Test code generation for various type scenarios
  - [ ] Add tests for edge cases (generics, nullables, nested types)

### Phase 10: Integration Testing
- [ ] Integration Tests Project
  - [ ] Create `MetaTypes.Integration.Tests.csproj`
  - [ ] End-to-end compilation and execution tests
  - [ ] Multi-project reference scenarios
  - [ ] Performance benchmarks vs. reflection
  - [ ] Memory usage validation

### Phase 11: Documentation and Polish
- [ ] Documentation
  - [ ] Write comprehensive README.md
  - [ ] Create getting-started guide
  - [ ] Generate API reference documentation
  - [ ] Add code examples and best practices
  - [ ] Document performance characteristics

### Phase 12: Package Preparation
- [ ] NuGet Package Validation
  - [ ] Test local package installation
  - [ ] Verify all dependencies are correct
  - [ ] Test package in clean environment
  - [ ] Validate package metadata and descriptions
  - [ ] Prepare for publication

## Priority Focus Areas

**Most Important (Core Functionality):**
1. **MetaTypes.Core** - The foundation interfaces and attributes
2. **Basic Source Generator** - Minimum viable generator that works
3. **Basic Usage Example** - Proves the concept works end-to-end

**Secondary Priority (Robustness):**
4. **Advanced Features** - Handles complex scenarios
5. **Advanced Usage Example** - Demonstrates full capabilities
6. **Testing Infrastructure** - Ensures reliability during refactoring

**Final Polish:**
7. **Integration Testing** - Validates real-world usage
8. **Documentation** - Makes the library usable by others
9. **Package Preparation** - Ready for distribution

This prioritization ensures you have a working prototype quickly, followed by the testing infrastructure needed for safe refactoring, and finally the polish needed for production use.

## Overview

MetaTypes is a .NET library that provides compile-time type metadata generation through source generators. It analyzes classes, structs, and records marked with the `[MetaType]` attribute and generates corresponding `MetaType` classes that offer a simplified, strongly-typed interface to type information without runtime reflection.

## Project Structure

```
MetaTypes/
├── src/
│   ├── MetaTypes.Core/
│   │   ├── MetaTypes.Core.csproj
│   │   ├── Attributes/
│   │   │   └── MetaTypeAttribute.cs
│   │   ├── Models/
│   │   │   ├── MetaType.cs
│   │   │   └── MetaTypeMember.cs
│   │   └── Interfaces/
│   │       ├── IMetaType.cs
│   │       └── IMetaTypeMember.cs
│   ├── MetaTypes.Generator/
│   │   ├── MetaTypes.Generator.csproj
│   │   ├── MetaTypeSourceGenerator.cs
│   │   ├── CodeGeneration/
│   │   │   ├── MetaTypeCodeGenerator.cs
│   │   │   └── MetaTypeMemberCodeGenerator.cs
│   │   ├── Analysis/
│   │   │   ├── TypeAnalyzer.cs
│   │   │   └── MemberAnalyzer.cs
│   │   └── Templates/
│   │       ├── MetaTypeTemplate.cs
│   │       └── MetaTypeMemberTemplate.cs
│   └── MetaTypes/
│       ├── MetaTypes.csproj
│       └── MetaTypes.cs (package reference aggregator)
├── tests/
│   ├── MetaTypes.Tests/
│   │   ├── MetaTypes.Tests.csproj
│   │   ├── SourceGeneratorTests.cs
│   │   ├── MetaTypeTests.cs
│   │   └── TestTypes/
│   │       ├── SimpleTestTypes.cs
│   │       ├── ComplexTestTypes.cs
│   │       └── GenericTestTypes.cs
│   └── MetaTypes.Integration.Tests/
│       ├── MetaTypes.Integration.Tests.csproj
│       └── EndToEndTests.cs
├── samples/
│   ├── BasicUsage/
│   │   ├── BasicUsage.csproj
│   │   └── Program.cs
│   └── AdvancedUsage/
│       ├── AdvancedUsage.csproj
│       └── Program.cs
├── docs/
│   ├── README.md
│   ├── getting-started.md
│   └── api-reference.md
├── build/
│   ├── Directory.Build.props
│   └── Directory.Build.targets
├── MetaTypes.sln
├── global.json
├── nuget.config
└── README.md
```

## Building the Project

### Prerequisites
- .NET 9 SDK
- C# 13 language features enabled

### Build Commands

```bash
# Restore packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack --configuration Release --output ./artifacts

# Build specific project
dotnet build src/MetaTypes.Core/

# Run source generator tests specifically
dotnet test tests/MetaTypes.Tests/ --filter Category=SourceGenerator
```

### Development Commands

```bash
# Watch and rebuild on file changes
dotnet watch build

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate documentation
dotnet tool restore
dotnet tool run docfx docfx.json
```

## Architecture

### Multi-Package Repository Structure

The solution produces three NuGet packages:

1. **MetaTypes.Core** - Base interfaces, attributes, and runtime models
2. **MetaTypes.Generator** - Source generator implementation (analyzer package)
3. **MetaTypes** - Meta-package that references both Core and Generator

### Package Dependencies

```xml
<!-- MetaTypes.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes.Core</PackageId>
  </PropertyGroup>
</Project>

<!-- MetaTypes.Generator.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes.Generator</PackageId>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="../MetaTypes.Core/MetaTypes.Core.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <None Include="tools/install.ps1" Pack="true" PackagePath="tools/install.ps1" />
    <None Include="tools/uninstall.ps1" Pack="true" PackagePath="tools/uninstall.ps1" />
  </ItemGroup>
  
  <ItemGroup>
    <Analyzer Include="$(OutputPath)/$(AssemblyName).dll" />
  </ItemGroup>
</Project>

<!-- MetaTypes.csproj (meta-package) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes</PackageId>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="MetaTypes.Core" Version="$(Version)" />
    <PackageReference Include="MetaTypes.Generator" Version="$(Version)" />
  </ItemGroup>
</Project>
```

### Source Code Generator Architecture

The source generator follows the incremental generator pattern:

1. **MetaTypeSourceGenerator** - Main generator class implementing `IIncrementalGenerator`
2. **TypeAnalyzer** - Analyzes syntax trees for `[MetaType]` attributed types
3. **MemberAnalyzer** - Analyzes properties and fields of identified types
4. **MetaTypeCodeGenerator** - Generates the MetaType class code
5. **MetaTypeMemberCodeGenerator** - Generates MetaTypeMember instances

## Core Interfaces and Models

### Marker Attribute

Use the standard `System.ComponentModel.DataAnnotations` attributes or create custom attributes for metadata.

### IMetaType Interface

```csharp
using System;
using System.Collections.Generic;

namespace MetaTypes.Core.Interfaces;

public interface IMetaType
{
    static abstract string TypeName { get; }
    static abstract string FullTypeName { get; }
    static abstract string Namespace { get; }
    static abstract Type RuntimeType { get; }
    static abstract IReadOnlyList<IMetaTypeMember> Members { get; }
    static abstract IReadOnlyList<Attribute> Attributes { get; }
    
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
    
    // MetaType cross-reference support - returns the actual IMetaType implementation or null
    static abstract IMetaType? PropertyMetaType { get; }
    
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

namespace MetaTypes.Generated;

public sealed class UserMetaType : IMetaType
{
    // Singleton instance
    public static readonly UserMetaType Instance = new();
    private UserMetaType() { }
    
    public static string TypeName => "User";
    public static string FullTypeName => "MyNamespace.User";
    public static string Namespace => "MyNamespace";
    public static Type RuntimeType => typeof(MyNamespace.User);
    
    private static readonly IReadOnlyList<IMetaTypeMember> _members = [
        UserMetaType_Name.Instance,
        UserMetaType_Age.Instance
    ];
    
    public static IReadOnlyList<IMetaTypeMember> Members => _members;
    
    private static readonly IReadOnlyList<Attribute> _attributes = [
        // Include attributes from the original class
    ];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    
    public static IMetaTypeMember? GetMember(string name) => name switch
    {
        "Name" => UserMetaType_Name.Instance,
        "Age" => UserMetaType_Age.Instance,
        _ => null
    };
    
    public static IReadOnlyList<IMetaTypeMember> GetMembersOfType<T>() =>
        _members.Where(m => typeof(T).IsAssignableFrom(m.GetPropertyType())).ToList();
    
    public static IReadOnlyList<IMetaTypeMember> GetMembersWithAttribute<T>() where T : Attribute =>
        _members.Where(m => m.GetAttribute<T>() is not null).ToList();
}
```

### Generated MetaTypeMember Class Template

For each property/field:

```csharp
// Generated for: public string Name { get; set; }

namespace MetaTypes.Generated;

public sealed class UserMetaType_Name : IMetaTypeMember
{
    public static string PropertyName => "Name";
    public static Type PropertyType => typeof(string);
    public static string PropertyTypeName => "string";
    public static bool IsEnum => false;
    public static bool IsNullable => false;
    public static bool IsList => false;
    public static bool IsGeneric => false;
    public static bool IsMetaType => false;
    
    private static readonly IReadOnlyList<Attribute> _attributes = [
        // Include any attributes from the original property
    ];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => null;
    
    // No MetaType reference for simple types
    public static IMetaType? PropertyMetaType => null;
    
    public static T? GetAttribute<T>() where T : Attribute =>
        _attributes.OfType<T>().FirstOrDefault();
    
    public static IEnumerable<T> GetAttributes<T>() where T : Attribute =>
        _attributes.OfType<T>();
}
```

### Generated MetaTypeMember Class Template

For each property/field:

```csharp
// Generated for: public string Name { get; set; }

namespace MetaTypes.Generated;

public sealed class UserMetaType_Name : IMetaTypeMember
{
    public static string PropertyName => "Name";
    public static Type PropertyType => typeof(string);
    public static string PropertyTypeName => "string";
    public static bool IsEnum => false;
    public static bool IsNullable => false;
    public static bool IsList => false;
    public static bool IsGeneric => false;
    public static bool IsMetaType => false;
    
    private static readonly IReadOnlyList<Attribute> _attributes = [
        // Include any attributes from the original property
    ];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => null;
    
    // No MetaType reference for simple types
    public static IMetaType? PropertyMetaType => null;
    
    public static T? GetAttribute<T>() where T : Attribute =>
        _attributes.OfType<T>().FirstOrDefault();
    
    public static IEnumerable<T> GetAttributes<T>() where T : Attribute =>
        _attributes.OfType<T>();
}
```

```csharp
// Generated for: public User? Clerk { get; set; } in SalesOrder class

namespace MetaTypes.Generated;

public sealed class SalesOrderMetaType_Clerk : IMetaTypeMember
{
    public static string PropertyName => "Clerk";
    public static Type PropertyType => typeof(User);
    public static string PropertyTypeName => "User?";
    public static bool IsEnum => false;
    public static bool IsNullable => true;
    public static bool IsList => false;
    public static bool IsGeneric => false;
    public static bool IsMetaType => true; // PropertyMetaType != null
    
    private static readonly IReadOnlyList<Attribute> _attributes = [
        // Include any attributes from the original property
    ];
    
    public static IReadOnlyList<Attribute> Attributes => _attributes;
    public static Type[]? GenericArguments => null;
    
    // Direct reference to UserMetaType as IMetaType
    public static IMetaType? PropertyMetaType => (IMetaType)typeof(UserMetaType);
    
    public static T? GetAttribute<T>() where T : Attribute =>
        _attributes.OfType<T>().FirstOrDefault();
    
    public static IEnumerable<T> GetAttributes<T>() where T : Attribute =>
        _attributes.OfType<T>();
}
```

```csharp
// Generated for: public List<OrderItem> Items { get; set; } where OrderItem has [MetaType]

namespace MetaTypes.Generated;

public sealed class SalesOrderMetaType_Items : IMetaTypeMember
{
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
    
    // Direct reference to OrderItemMetaType as IMetaType (for the generic argument)
    public static IMetaType? PropertyMetaType => (IMetaType)typeof(OrderItemMetaType);
    
    public static T? GetAttribute<T>() where T : Attribute =>
        _attributes.OfType<T>().FirstOrDefault();
    
    public static IEnumerable<T> GetAttributes<T>() where T : Attribute =>
        _attributes.OfType<T>();
}
```

**Usage Examples:**

```csharp
// Simple property - no MetaType reference
if (UserMetaType_Name.PropertyMetaType is null)
{
    Console.WriteLine($"{UserMetaType_Name.PropertyName} is not a MetaType");
}

// MetaType property - direct access to referenced MetaType
if (SalesOrderMetaType_Clerk.PropertyMetaType is IMetaType userMetaType)
{
    Console.WriteLine($"Clerk property references: {userMetaType.TypeName}");
    Console.WriteLine($"Namespace: {userMetaType.Namespace}");
    Console.WriteLine($"Members: {userMetaType.Members.Count}");
    
    // Access members of the referenced MetaType
    foreach (var member in userMetaType.Members)
    {
        Console.WriteLine($"  - {member.PropertyName}: {member.PropertyTypeName}");
    }
}

// Collection of MetaTypes
if (SalesOrderMetaType_Items.IsMetaType && SalesOrderMetaType_Items.PropertyMetaType is IMetaType itemMetaType)
{
    Console.WriteLine($"Items collection contains: {itemMetaType.TypeName}");
}

// Generic helper method
static void AnalyzeProperty<T>() where T : IMetaTypeMember
{
    Console.WriteLine($"Property: {T.PropertyName} ({T.PropertyTypeName})");
    
    if (T.PropertyMetaType is IMetaType metaType)
    {
        Console.WriteLine($"  References MetaType: {metaType.TypeName}");
        Console.WriteLine($"  Member count: {metaType.Members.Count}");
    }
    else
    {
        Console.WriteLine($"  Simple type: {T.PropertyType.Name}");
    }
}

```csharp
// Generated for: public List<OrderItem> Items { get; set; } where OrderItem has [MetaType]

namespace MetaTypes.Generated;

public sealed class SalesOrderMetaType_Items : IMetaTypeMember
{
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
    
    // Direct reference to OrderItemMetaType (for the generic argument)
    public static IMetaType? PropertyMetaType => OrderItemMetaType.Instance;
    
    public static T? GetAttribute<T>() where T : Attribute =>
        _attributes.OfType<T>().FirstOrDefault();
    
    public static IEnumerable<T> GetAttributes<T>() where T : Attribute =>
        _attributes.OfType<T>();
}
```

**Usage Examples:**

```csharp
// Direct access to the referenced MetaType
if (SalesOrderMetaType_Clerk.IsMetaType)
{
    var userMetaType = SalesOrderMetaType_Clerk.PropertyMetaType; // UserMetaType.Instance
    
    // Can access all UserMetaType properties directly
    var userTypeName = userMetaType.TypeName; // "User"
    var userMembers = userMetaType.Members;
    var userNamespace = userMetaType.Namespace;
}

// For collections of MetaTypes
if (SalesOrderMetaType_Items.IsMetaType && SalesOrderMetaType_Items.IsList)
{
    var itemMetaType = SalesOrderMetaType_Items.PropertyMetaType; // OrderItemMetaType.Instance
    var itemTypeName = itemMetaType.TypeName; // "OrderItem"
    var itemMembers = itemMetaType.Members;
}

// Generic helper method using the interface
static void ProcessMetaTypeMember<T>(T member) where T : IMetaTypeMember
{
    if (T.IsMetaType && T.PropertyMetaType is not null)
    {
        Console.WriteLine($"Property {T.PropertyName} references MetaType: {T.PropertyMetaType.TypeName}");
        foreach (var subMember in T.PropertyMetaType.Members)
        {
            Console.WriteLine($"  - {subMember.PropertyName}: {subMember.PropertyTypeName}");
        }
    }
}
```

**Alternative Usage Pattern:**
Since everything is static, you can also access members directly without collections:

```csharp
// Direct static access - most efficient
var userName = UserMetaType_Name.PropertyName;
var userAge = UserMetaType_Age.PropertyType;

// Or via the MetaType class
var nameType = UserMetaType.GetMember("Name");
```

## Source Generator Implementation Requirements

### Generator Features

1. **Incremental Generation** - Use `IIncrementalGenerator` for performance
2. **Syntax Provider** - Register for class/struct/record declarations
3. **Attribute Detection** - Filter for `[MetaType]` attributed types
4. **Member Analysis** - Analyze properties and fields based on attribute settings
5. **Nested Type Support** - Handle nested classes and generic types
6. **Cross-Reference Resolution** - Resolve MetaType references between types
7. **Error Handling** - Provide meaningful diagnostics for invalid usage

### Code Generation Rules

1. **Naming Convention**: `{TypeName}MetaType` for the generated class
2. **Namespace Strategy**: 
   - **Default behavior**: Analyze all `[MetaType]` attributed types in the project
   - Find the common namespace prefix (e.g., `Samples.Entities` from `Samples.Entities.User`, `Samples.Entities.SalesOrder`)
   - Generate all MetaTypes in `{CommonNamespace}.MetaTypes` namespace
   - **Configuration override**: If `MetaTypesNamespace` MSBuild property is set, use that instead
   - If no common namespace exists, use `{ProjectRootNamespace}.MetaTypes`
   - Handle edge cases: single namespace, global namespace, mixed namespaces
3. **Accessibility**: Generated classes are `public static`
4. **Singleton Pattern**: No instances - pure static classes with static abstract interface implementation
5. **Member Naming**: `{TypeName}MetaType_{PropertyName}` for member classes
6. **Null Safety**: Handle nullable reference types correctly
7. **Generic Support**: Properly represent generic types and their constraints
8. **Cross-References**: MetaType references use the same generated namespace for efficiency

### Type Analysis Requirements

1. **Property Detection**: Analyze public properties by default
2. **Field Detection**: Include fields only if `IncludeFields = true`
3. **Private Members**: Include private members only if `IncludePrivateMembers = true`
4. **Attribute Preservation**: Copy all attributes from source members
5. **Type Classification**: Correctly identify enums, nullables, lists, generics
6. **MetaType References**: Detect when property types are also MetaTypes
7. **Cross-Reference Resolution**: Handle MetaType references in:
   - Direct properties: `public User? Clerk { get; set; }`
   - Generic collections: `public List<OrderItem> Items { get; set; }`
   - Nested generics: `public Dictionary<string, User> UserMap { get; set; }`
   - Nullable MetaTypes: Properly set `IsNullable = true` for `User?`
8. **PropertyMetaType Logic**: 
   - For direct MetaType properties: Points to the MetaType class
   - For generic collections with MetaType arguments: Points to the inner MetaType class
   - For non-MetaType properties: `null`
   - For complex generics: Points to the first MetaType argument found
9. **Namespace Analysis**: 
   - Collect all `[MetaType]` attributed types and their namespaces
   - Calculate common namespace prefix across all types
   - Generate consistent namespace for all MetaTypes in the project
   - Handle cross-project references if MetaTypes span multiple assemblies

## Testing Requirements

### Unit Tests

1. **Generator Tests**: Verify correct code generation for various input scenarios
2. **Analyzer Tests**: Test attribute detection and member analysis
3. **Type Resolution Tests**: Verify MetaType cross-references work correctly
4. **Edge Case Tests**: Handle nullable types, generics, nested types
5. **Performance Tests**: Ensure generator performance is acceptable

### Integration Tests

1. **End-to-End Tests**: Full compilation and usage scenarios
2. **Multi-Project Tests**: Verify cross-assembly MetaType references
3. **Runtime Tests**: Verify generated code executes correctly
4. **Incremental Tests**: Test incremental generation scenarios

### Test Coverage Requirements

- Minimum 90% code coverage for generator logic
- All public API surface area must be tested
- Performance benchmarks for large codebases
- Memory usage validation for generator

## Documentation Requirements

1. **API Documentation**: XML documentation for all public APIs
2. **Usage Examples**: Comprehensive examples in samples folder
3. **Getting Started Guide**: Step-by-step setup and basic usage
4. **Advanced Scenarios**: Complex usage patterns and best practices
5. **Performance Guide**: Best practices for optimal generator performance
6. **Migration Guide**: If applicable, migration from previous versions

## Modern .NET Coding Standards

### C# 13 and .NET 9 Language Features

The MetaTypes library should leverage modern C# language features for clean, performant code:

#### Static Abstract Interface Members (C# 11+)
```csharp
// Use static abstract interface members for compile-time contracts
public interface IMetaType
{
    static abstract string TypeName { get; }
    static abstract Type RuntimeType { get; }
    static abstract IReadOnlyList<IMetaTypeMember> Members { get; }
}

// Implementation
public sealed class UserMetaType : IMetaType
{
    public static string TypeName => "User";
    public static Type RuntimeType => typeof(User);
    public static IReadOnlyList<IMetaTypeMember> Members => [typeof(UserMetaType_Name)];
}
```

#### Collection Initializers and Expressions
```csharp
// Use collection expressions (C# 12/13)
private static readonly IReadOnlyList<IMetaTypeMember> _members = [
    new UserMetaType_Name(),
    new UserMetaType_Age()
];

// Use collection expressions for arrays
private static readonly Attribute[] _attributes = [
    new MetaTypeAttribute(),
    new RequiredAttribute()
];

// Use collection expressions in methods
public IEnumerable<IMetaTypeMember> GetMembersOfType<T>() =>
    _members.Where(m => typeof(T).IsAssignableFrom(m.PropertyType));
```

#### Target-Typed New Expressions
```csharp
// Use target-typed new expressions
private static readonly IReadOnlyList<IMetaTypeMember> _members = new[]
{
    new UserMetaType_Name(),
    new UserMetaType_Age()
};

// In method returns
public IMetaTypeMember? GetMember(string name) => 
    _members.FirstOrDefault(m => m.PropertyName == name) ?? null;
```

#### Primary Constructors (C# 12)
```csharp
// Use primary constructors for simple data classes
public sealed class MetaTypeMember(
    string propertyName,
    Type propertyType,
    bool isEnum = false,
    bool isNullable = false,
    bool isList = false) : IMetaTypeMember
{
    public string PropertyName { get; } = propertyName;
    public Type PropertyType { get; } = propertyType;
    public bool IsEnum { get; } = isEnum;
    public bool IsNullable { get; } = isNullable;
    public bool IsList { get; } = isList;
    
    // Additional computed properties
    public string PropertyTypeName => PropertyType.Name;
    public bool IsGeneric => PropertyType.IsGenericType;
}
```

#### Pattern Matching and Switch Expressions
```csharp
// Use pattern matching for type analysis
public bool IsMetaType => PropertyType.GetCustomAttribute<MetaTypeAttribute>() is not null;

// Use switch expressions for type classification
public string GetTypeCategory() => PropertyType switch
{
    _ when IsEnum => "Enum",
    _ when IsList => "Collection",
    _ when IsGeneric => "Generic",
    _ when IsMetaType => "MetaType",
    _ => "Simple"
};

// Pattern matching in member analysis
public static bool IsMetaTypeCandidate(TypeDeclarationSyntax typeDecl) =>
    typeDecl.AttributeLists
        .SelectMany(al => al.Attributes)
        .Any(attr => attr.Name.ToString() is "MetaType" or "MetaTypeAttribute");
```

#### Record Types and Init-Only Properties
```csharp
// Use records for immutable data transfer objects
public sealed record TypeAnalysisResult(
    string TypeName,
    string FullTypeName,
    string Namespace,
    IReadOnlyList<MemberAnalysisResult> Members,
    IReadOnlyList<Attribute> Attributes);

public sealed record MemberAnalysisResult
{
    public required string Name { get; init; }
    public required Type Type { get; init; }
    public required bool IsEnum { get; init; }
    public required bool IsNullable { get; init; }
    public required bool IsList { get; init; }
    public IReadOnlyList<Attribute> Attributes { get; init; } = [];
}
```

#### Required Properties and Constructors
```csharp
// Use required properties for essential data
public class MetaTypeGenerationContext
{
    public required string TypeName { get; init; }
    public required string FullTypeName { get; init; }
    public required string Namespace { get; init; }
    public required IReadOnlyList<MemberInfo> Members { get; init; }
    public IReadOnlyList<Attribute> Attributes { get; init; } = [];
}
```

#### File-Scoped Namespaces
```csharp
// Use file-scoped namespaces throughout the project
namespace MetaTypes.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Record)]
public sealed class MetaTypeAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IncludePrivateMembers { get; set; } = false;
    public bool IncludeFields { get; set; } = false;
}
```

#### Nullable Reference Types
```csharp
// Enable nullable reference types in all projects
#nullable enable

// Use nullable annotations appropriately
public interface IMetaType
{
    string TypeName { get; }
    string FullTypeName { get; }
    string Namespace { get; }
    Type RuntimeType { get; }
    IReadOnlyList<IMetaTypeMember> Members { get; }
    IReadOnlyList<Attribute> Attributes { get; }
    
    IMetaTypeMember? GetMember(string name);
    IEnumerable<IMetaTypeMember> GetMembersOfType<T>();
    IEnumerable<IMetaTypeMember> GetMembersWithAttribute<T>() where T : Attribute;
}
```

#### String Interpolation and Raw String Literals
```csharp
// Use string interpolation for code generation
private static string GenerateMetaTypeClass(TypeAnalysisResult analysis) => $"""
    namespace MetaTypes.Generated;

    public sealed class {{analysis.TypeName}}MetaType : IMetaType
    {
        public static readonly {{analysis.TypeName}}MetaType Instance = new();
        
        private {{analysis.TypeName}}MetaType() { }
        
        public string TypeName => "{{analysis.TypeName}}";
        public string FullTypeName => "{{analysis.FullTypeName}}";
        public string Namespace => "{{analysis.Namespace}}";
        public Type RuntimeType => typeof({{analysis.FullTypeName}});
        
        private static readonly IReadOnlyList<IMetaTypeMember> _members = [
            {{string.Join(",\n            ", analysis.Members.Select(m => $"new {analysis.TypeName}MetaType_{m.Name}()"))}}
        ];
        
        public IReadOnlyList<IMetaTypeMember> Members => _members;
        
        // Additional implementation...
    }
    """;
```

#### Global Using Statements
```csharp
// In GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using MetaTypes.Core.Attributes;
global using MetaTypes.Core.Interfaces;
```

#### Performance-Oriented Features
```csharp
// Use ReadOnlySpan<T> for string operations where appropriate
public static ReadOnlySpan<char> GetTypeNameSpan(string fullTypeName)
{
    int lastDotIndex = fullTypeName.LastIndexOf('.');
    return lastDotIndex == -1 ? fullTypeName.AsSpan() : fullTypeName.AsSpan(lastDotIndex + 1);
}

// Use stackalloc for small collections
Span<char> buffer = stackalloc char[256];
var written = fullTypeName.AsSpan().CopyTo(buffer);

// Use string.Create for efficient string building
public static string CreateMemberName(string typeName, string memberName) =>
    string.Create(typeName.Length + memberName.Length + 10, (typeName, memberName), 
        static (span, state) =>
        {
            var (type, member) = state;
            var pos = 0;
            type.AsSpan().CopyTo(span[pos..]);
            pos += type.Length;
            "MetaType_".AsSpan().CopyTo(span[pos..]);
            pos += 9;
            member.AsSpan().CopyTo(span[pos..]);
        });
```

#### Immutable Collections
```csharp
// Use System.Collections.Immutable for thread-safe collections
using System.Collections.Immutable;

public sealed class MetaTypeCache
{
    private static readonly ImmutableDictionary<Type, IMetaType> _cache = 
        ImmutableDictionary<Type, IMetaType>.Empty;
    
    public static ImmutableArray<IMetaType> GetAllMetaTypes() => 
        _cache.Values.ToImmutableArray();
}
```

### Code Style Guidelines

#### Naming Conventions
- Use **PascalCase** for public members, types, and namespaces
- Use **camelCase** for parameters, local variables, and private fields
- Use **_camelCase** for private fields (with underscore prefix)
- Use **UPPER_CASE** for constants
- Use **IPascalCase** for interfaces
- Use **TPascalCase** for generic type parameters

#### Code Organization
- One class per file
- File-scoped namespaces
- Order members: constants, fields, properties, constructors, methods
- Group related functionality together
- Use regions sparingly, only for large classes

#### Error Handling
```csharp
// Use modern exception handling patterns
public IMetaTypeMember? GetMember(string name)
{
    ArgumentException.ThrowIfNullOrEmpty(name);
    
    return _members.FirstOrDefault(m => m.PropertyName == name);
}

// Use ArgumentNullException.ThrowIfNull for null checks
public void ProcessType(Type type)
{
    ArgumentNullException.ThrowIfNull(type);
    
    // Implementation...
}
```

#### Asynchronous Programming
```csharp
// Use ValueTask for high-performance async operations
public static async ValueTask<IReadOnlyList<TypeAnalysisResult>> AnalyzeTypesAsync(
    IEnumerable<TypeDeclarationSyntax> types,
    CancellationToken cancellationToken = default)
{
    var results = new List<TypeAnalysisResult>();
    
    await foreach (var analysis in AnalyzeTypesAsyncEnumerable(types, cancellationToken))
    {
        results.Add(analysis);
    }
    
    return results;
}

// Use IAsyncEnumerable for streaming results
public static async IAsyncEnumerable<TypeAnalysisResult> AnalyzeTypesAsyncEnumerable(
    IEnumerable<TypeDeclarationSyntax> types,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (var type in types)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var analysis = await AnalyzeTypeAsync(type, cancellationToken);
        yield return analysis;
    }
}
```

### Project Configuration for Modern Features

#### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

#### .editorconfig
```ini
root = true

[*.cs]
# Use file-scoped namespaces
csharp_style_namespace_declarations = file_scoped
# Use collection expressions
dotnet_style_collection_initializer = true
# Use target-typed new
csharp_style_implicit_object_creation_when_type_is_apparent = true
# Use pattern matching
csharp_style_pattern_matching_over_is_with_cast_check = true
csharp_style_pattern_matching_over_as_with_null_check = true
# Use expression-bodied members
csharp_style_expression_bodied_methods = true
csharp_style_expression_bodied_properties = true
```

These modern coding standards ensure the MetaTypes library uses the latest C# 13 and .NET 9 features for optimal performance, readability, and maintainability.

### Packaging and Distribution

### NuGet Package Metadata

```xml
<PropertyGroup>
  <PackageId>MetaTypes</PackageId>
  <Version>1.0.0</Version>
  <Authors>Your Name</Authors>
  <Description>Compile-time type metadata generation for .NET</Description>
  <PackageTags>sourcegenerator;reflection;metadata;compile-time</PackageTags>
  <PackageProjectUrl>https://github.com/yourorg/metatypes</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/yourorg/metatypes</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
</PropertyGroup>
```

### Build Configuration

- **Target Framework**: 
  - .NET 9 for Core and Meta-package (leveraging modern features)
  - .NET Standard 2.0 for Generator (Roslyn compatibility requirement)
- **Language Version**: C# 13 for latest features
- **Nullable**: Enable for all projects
- **Warnings as Errors**: Enable for production builds
- **Deterministic Builds**: Enable for reproducible packages

This specification provides a complete blueprint for implementing the MetaTypes library with all necessary architectural decisions, code structure, and implementation requirements clearly defined.