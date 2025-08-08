# MetaTypes.Generator.EfCore.Common

## Overview
This folder contains EF Core-specific shared code for source generators. It's not a standalone project - files are included via LinkBase patterns.

## Key Components

### EF Core Discovery
- **EfCoreDiscoveryMethods** - Collection of methods specifically designed for discovering Entity Framework Core entities, DbContexts, and related types

## Usage
Files from this folder are automatically included in EF Core generators using:
```xml
<Compile Include="../MetaTypes.Generator.EfCore.Common/**/*.cs" LinkBase="EfCore" />
```

## Usage in EF Core Source Generators
Include this shared code in your EF Core-specific source generator projects using the LinkBase approach:
```xml
<Compile Include="../MetaTypes.Generator.EfCore.Common/**/*.cs" LinkBase="EfCore" />
```

Use the EfCoreDiscoveryMethods in your generator's type discovery logic:
```csharp
protected override IEnumerable<DiscoveredType> DiscoverTypes(
    Compilation compilation, 
    ImmutableArray<AdditionalText> additionalFiles, 
    CancellationToken cancellationToken)
{
    // Use EfCoreDiscoveryMethods for EF-specific discovery
    return EfCoreDiscoveryMethods.DiscoverEfCoreTypes(compilation);
}
```

## Files
- **EfCoreDiscoveryMethods** - EF Core entity discovery logic