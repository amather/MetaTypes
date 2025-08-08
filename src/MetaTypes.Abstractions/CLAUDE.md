# MetaTypes.Abstractions

## Overview
This project contains the core abstractions, interfaces, and attributes for the MetaTypes library. This is a shared library that can be referenced by both runtime code and source generators.

## Key Components

### Core Interfaces
- **IMetaType** - Core interface representing a meta type with properties like Name, FullName, etc.
- **IMetaTypeMember** - Represents a member (property/field) of a meta type
- **IMetaTypeEfCore** - Entity Framework Core specific extensions to IMetaType
- **IMetaTypeMemberEfCore** - EF Core specific extensions to IMetaTypeMember
- **IMetaTypeProvider** - Provider interface for retrieving meta types

### Configuration Interfaces
- **IAssemblyNameProvider** - Interface for providing assembly names during generation
- **IGeneratorConfiguration** - Configuration interface for generator settings

### Attributes
- **MetaTypeAttribute** - Attribute used to mark types that should have meta types generated

### Service Extensions
- **ServiceCollectionExtensions** - Extensions for dependency injection setup
- **ServiceProviderExtensions** - Extensions for service provider usage

## Project Configuration
- **Target Framework**: netstandard2.0;net8.0 (multi-targeting for broad compatibility)
- **Dependencies**: 
  - Microsoft.Extensions.DependencyInjection.Abstractions
  - Microsoft.CodeAnalysis.CSharp (for compilation-time features)

## Usage
This library is included in source generators using the LinkBase approach:
```xml
<Compile Include="../MetaTypes.Abstractions/**/*.cs" LinkBase="Abstractions" />
```

## Commands
- `dotnet build` - Build the library
- `dotnet pack` - Create NuGet package