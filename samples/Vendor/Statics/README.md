# Statics Vendor Samples

This directory contains sample projects demonstrating the Statics vendor extensions for MetaTypes generation.

## ✅ Status: Implementation Complete (August 12th, 2025)

The Statics vendor functionality has been fully implemented with complete architecture including discovery methods, vendor generators, and abstractions.

## Sample Projects

### Sample.Statics.ServiceMethod
**Purpose**: Demonstrates Statics vendor generation for static service classes with attributed methods.

**Features**:
- ✅ Static service class discovery with `StaticsServiceMethodAttribute`
- ✅ Both syntax-based AND cross-assembly discovery support
- ✅ Complete method metadata generation including:
  - Method attributes with constructor and named arguments
  - Parameter attributes with complete attribute information
  - Return type and parameter type information
- ✅ Self-contained project with example service classes
- ✅ Integration with Statics.ServiceBroker.Attributes

**Generated Files**:
- Base MetaType classes for static service classes
- Statics vendor extensions implementing `IMetaTypeStatics`
- Complete service method metadata implementing `IStaticsServiceMethod`

**Configuration**: Full Statics vendor configuration with base types enabled.

## Generated Statics Extensions

When static classes are discovered by Statics discovery methods, the vendor generator creates additional partial classes implementing `IMetaTypeStatics`:

```csharp
public partial class UserServicesMetaType : IMetaTypeStatics
{
    public IReadOnlyList<IStaticsServiceMethod> ServiceMethods => [
        new UserServicesServiceMethodGetUserById(),
        new UserServicesServiceMethodCreateUser(),
        new UserServicesServiceMethodUpdateUserStatus(),
    ];
}

public class UserServicesServiceMethodGetUserById : IStaticsServiceMethod
{
    public string MethodName => "GetUserById";
    public string ReturnType => "string";
    
    public IReadOnlyList<IStaticsAttributeInfo> MethodAttributes => [
        new UserServicesMethodGetUserByIdAttributeStaticsServiceMethod(),
    ];
    
    public IReadOnlyList<IStaticsParameterInfo> Parameters => [
        new UserServicesMethodGetUserByIdParameteruserId(),
    ];
}
```

## Configuration Examples

### Statics Vendor Configuration
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": false,
      "Methods": [
        "MetaTypes.Attribute",
        "Statics.ServiceMethod"
      ]
    },
    "EnabledVendors": ["Statics"],
    "VendorConfigs": {
      "Statics": {
        "RequireBaseTypes": true,
        "IncludeParameterAttributes": true,
        "IncludeMethodAttributes": true
      }
    }
  }
}
```

## Statics Discovery Methods

### Statics.ServiceMethod
- **Purpose**: Discovers static classes containing static methods with `StaticsServiceMethodAttribute`
- **Type**: Both syntax-based AND cross-assembly discovery
- **Scope**: Current compilation and referenced assemblies
- **Criteria**: 
  - Static classes (public or internal)
  - Containing static methods with `[StaticsServiceMethod]` attribute
  - Methods must be public or internal
- **Example**: 
```csharp
[MetaType]
public static class UserServices
{
    [StaticsServiceMethod]
    public static string GetUserById(int userId) { ... }
}
```

## Running the Samples

### Prerequisites
- .NET 9.0 SDK (or compatible)
- Statics.ServiceBroker reference for `StaticsServiceMethodAttribute`

### Build and Run
```bash
# ServiceMethod sample
cd Sample.Statics.ServiceMethod
dotnet run
```

### Expected Output
The ServiceMethod sample will:
1. Execute example static service methods
2. Display method execution results
3. Generate MetaTypes and Statics vendor extensions with complete method metadata

## Generated Metadata Features

### Method Attributes
- Complete attribute type information
- Constructor arguments with types and values
- Named arguments (properties/fields) with types and values
- Nested attribute argument classes for complex scenarios

### Parameter Attributes
- All parameter-level attributes (e.g., `[Required]`, `[Range]`, custom attributes)
- Complete attribute information for each parameter
- Support for validation attributes and custom parameter attributes

### Type Information
- Method return types
- Parameter types with full type names
- Static class discovery and metadata

## Architecture Notes

### Vendor Implementation (August 2025)
- **Discovery Methods**: Both `StaticsServiceMethodDiscoveryMethod` supports syntax-based and cross-assembly discovery
- **Vendor Generator**: `StaticsServiceMethodVendorGenerator` creates comprehensive method metadata
- **Abstractions**: Complete interface hierarchy in `MetaTypes.Abstractions.Vendor.Statics`
- **Configuration**: Self-configuring vendor with `StaticsServiceMethodConfig` options

### Generated Code Structure
- **Naming Convention**: `{AssemblyName}_{ClassName}MetaTypeStatics.g.cs`
- **Interface Implementation**: Generated classes implement `IMetaTypeStatics`
- **Method Metadata**: Each discovered method gets detailed metadata classes
- **Attribute Processing**: Complete attribute information with arguments and values

### Discovery and Filtering
- **Discovery Method**: `Statics.ServiceMethod` finds static classes with attributed methods
- **Filtering**: Only processes static classes containing methods with `StaticsServiceMethodAttribute`
- **Cross-Assembly Support**: Works with both local classes and external assemblies
- **Attribute Support**: Processes method and parameter attributes comprehensively

### Service Method Features
- **Method Discovery**: Finds public/internal static methods with `[StaticsServiceMethod]`
- **Attribute Metadata**: Complete method and parameter attribute information
- **Type Safety**: Full type information for return types and parameters
- **Extensible**: Supports custom attributes and complex attribute scenarios