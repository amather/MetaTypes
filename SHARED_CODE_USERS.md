# MetaTypes Shared Code Guide

## Overview

This document explains how to use MetaTypes shared code in your projects. It covers both implementing new MetaTypes source generators and integrating MetaTypes functionality into external projects.

## Available Shared Components

- **MetaTypes.Abstractions** - Runtime library with core interfaces and attributes
- **MetaTypes.Generator.Common** - Shared source generator utilities (not a real project)
- **MetaTypes.Generator.EfCore.Common** - EF Core-specific generator utilities (not a real project)

## Problem

Source generators must be self-contained assemblies and cannot use traditional project references. External projects wanting to reuse MetaTypes shared code face similar challenges. Previously, manual lists of shared files were required:

```xml
<Compile Include="../MetaTypes.Generator.Common/CoreMetaTypeGenerator.cs" Link="Common/CoreMetaTypeGenerator.cs" />
<Compile Include="../MetaTypes.Generator.Common/DiscoveredType.cs" Link="Common/DiscoveredType.cs" />
<!-- ... 8+ more manual entries -->
```

This approach requires maintenance whenever files are added/removed from shared directories.

## Preferred Solution: LinkBase with Wildcards

Use `LinkBase` metadata with wildcard patterns to automatically include all shared files:

```xml
<ItemGroup>
  <Compile Include="../MetaTypes.Generator.Common/**/*.cs" 
           Exclude="../MetaTypes.Generator.Common/obj/**/*.cs;../MetaTypes.Generator.Common/bin/**/*.cs" 
           LinkBase="Common" />
  <Compile Include="../MetaTypes.Abstractions/**/*.cs" 
           Exclude="../MetaTypes.Abstractions/obj/**/*.cs;../MetaTypes.Abstractions/bin/**/*.cs;../MetaTypes.Abstractions/ServiceCollectionExtensions.cs;../MetaTypes.Abstractions/ServiceProviderExtensions.cs" 
           LinkBase="Abstractions" />
  <Compile Include="../MetaTypes.Generator.EfCore.Common/**/*.cs" 
           Exclude="../MetaTypes.Generator.EfCore.Common/obj/**/*.cs;../MetaTypes.Generator.EfCore.Common/bin/**/*.cs" 
           LinkBase="EfCore" />
</ItemGroup>
```

### How LinkBase Works

`LinkBase` automatically generates `Link` metadata using the formula:
```
Link = %(LinkBase) + %(RecursiveDir) + %(Filename)%(Extension)
```

**Example**:
- File: `../MetaTypes.Generator.Common/Utils/Helper.cs`
- `LinkBase="Common"`
- Result: `Link="Common/Utils/Helper.cs"`

## Benefits

1. **Zero Maintenance**: New files are automatically included
2. **Preserves Structure**: Subdirectories are maintained in Solution Explorer
3. **Cross-Codebase Sharing**: Can be packaged in `.targets` files for other repositories
4. **Modern Tooling**: Uses SDK-style project features (available since .NET Core SDK 2.0)

## Important: Exclude Patterns Required

The `Exclude` attribute is essential to prevent compilation errors:

1. **Build Artifacts** (`obj/**/*.cs;bin/**/*.cs`): Prevents including auto-generated AssemblyInfo.cs files that cause duplicate attribute errors
2. **Runtime Dependencies**: Files like `ServiceCollectionExtensions.cs` use Microsoft.Extensions.DependencyInjection which isn't available in netstandard2.0 source generators

## For External Projects and Cross-Codebase Sharing

### Option 1: Direct Path References (Submodule/Local Copy)

If you have the MetaTypes repository as a submodule or local copy:

```xml
<ItemGroup>
  <Compile Include="path/to/MetaTypes/src/MetaTypes.Generator.Common/**/*.cs" 
           Exclude="path/to/MetaTypes/src/MetaTypes.Generator.Common/obj/**/*.cs;path/to/MetaTypes/src/MetaTypes.Generator.Common/bin/**/*.cs" 
           LinkBase="MetaTypes/Common" />
  <Compile Include="path/to/MetaTypes/src/MetaTypes.Abstractions/**/*.cs" 
           Exclude="path/to/MetaTypes/src/MetaTypes.Abstractions/obj/**/*.cs;path/to/MetaTypes/src/MetaTypes.Abstractions/bin/**/*.cs;path/to/MetaTypes/src/MetaTypes.Abstractions/ServiceCollectionExtensions.cs;path/to/MetaTypes/src/MetaTypes.Abstractions/ServiceProviderExtensions.cs" 
           LinkBase="MetaTypes/Abstractions" />
  <Compile Include="path/to/MetaTypes/src/MetaTypes.Generator.EfCore.Common/**/*.cs" 
           Exclude="path/to/MetaTypes/src/MetaTypes.Generator.EfCore.Common/obj/**/*.cs;path/to/MetaTypes/src/MetaTypes.Generator.EfCore.Common/bin/**/*.cs" 
           LinkBase="MetaTypes/EfCore" />
</ItemGroup>
```

### Option 2: MSBuild Targets File (Recommended for Distribution)

Create `MetaTypes.Generator.Shared.targets` in your distribution:

```xml
<Project>
  <ItemGroup>
    <Compile Include="$(MSBuildThisFileDirectory)src/MetaTypes.Generator.Common/**/*.cs" 
             Exclude="$(MSBuildThisFileDirectory)src/MetaTypes.Generator.Common/obj/**/*.cs;$(MSBuildThisFileDirectory)src/MetaTypes.Generator.Common/bin/**/*.cs" 
             LinkBase="MetaTypes/Common" />
    <Compile Include="$(MSBuildThisFileDirectory)src/MetaTypes.Abstractions/**/*.cs" 
             Exclude="$(MSBuildThisFileDirectory)src/MetaTypes.Abstractions/obj/**/*.cs;$(MSBuildThisFileDirectory)src/MetaTypes.Abstractions/bin/**/*.cs;$(MSBuildThisFileDirectory)src/MetaTypes.Abstractions/ServiceCollectionExtensions.cs;$(MSBuildThisFileDirectory)src/MetaTypes.Abstractions/ServiceProviderExtensions.cs" 
             LinkBase="MetaTypes/Abstractions" />
    <Compile Include="$(MSBuildThisFileDirectory)src/MetaTypes.Generator.EfCore.Common/**/*.cs" 
             Exclude="$(MSBuildThisFileDirectory)src/MetaTypes.Generator.EfCore.Common/obj/**/*.cs;$(MSBuildThisFileDirectory)src/MetaTypes.Generator.EfCore.Common/bin/**/*.cs" 
             LinkBase="MetaTypes/EfCore" />
  </ItemGroup>
</Project>
```

External projects can then import this:
```xml
<Import Project="path/to/MetaTypes.Generator.Shared.targets" />
```

### Option 3: NuGet Package Integration (Future)

For NuGet package distribution, the targets file can be included in the package's `build` folder and automatically imported.

## Requirements

- SDK-style projects (`<Project Sdk="Microsoft.NET.Sdk">`) ✅
- .NET Core SDK 2.0+ (.NET 9 in our case) ✅
- Source generators targeting netstandard2.0+ ✅

## Migration

Replace manual `<Compile Include=` entries with the wildcard patterns above. The LinkBase approach will generate identical `Link` paths, maintaining the same Solution Explorer organization.