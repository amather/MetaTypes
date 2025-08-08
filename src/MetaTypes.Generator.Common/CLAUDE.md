# MetaTypes.Generator.Common

## Overview
This folder contains shared source generator code that gets linked into actual generator projects. It's not a standalone project - files are included via LinkBase patterns.

## Key Components

### Core Generator Classes
- **CoreMetaTypeGenerator** - Abstract base class that source generators should extend. Provides common generation logic and structure.
- **DiscoveredType** - Model representing a discovered type during the source generation process
- **UnifiedTypeDiscovery** - Utility class providing unified type discovery mechanisms
- **CommonDiscoveryMethods** - Collection of common methods for discovering types in compilations

### Configuration
- **GeneratorConfiguration** - Implementation of IGeneratorConfiguration for managing generator settings
- **AssemblyNameProvider** - Implementation of IAssemblyNameProvider for providing assembly names

## Usage
Files from this folder are automatically included in generator projects using:
```xml
<Compile Include="../MetaTypes.Generator.Common/**/*.cs" LinkBase="Common" />
```

## Usage in Source Generators
Include this shared code in your source generator projects using the LinkBase approach:
```xml
<Compile Include="../MetaTypes.Generator.Common/**/*.cs" LinkBase="Common" />
```

Then extend CoreMetaTypeGenerator:
```csharp
[Generator]
public class YourSourceGenerator : CoreMetaTypeGenerator
{
    protected override string GetNamespace(string assemblyName) =>
        $"{assemblyName}.Generated.MetaTypes";

    protected override IEnumerable<DiscoveredType> DiscoverTypes(
        Compilation compilation, 
        ImmutableArray<AdditionalText> additionalFiles, 
        CancellationToken cancellationToken)
    {
        // Your discovery logic here
    }
}
```

## Files
- **CoreMetaTypeGenerator** - Base class for generators
- **DiscoveredType** - Model for discovered types
- **UnifiedTypeDiscovery** - Type discovery utilities
- **CommonDiscoveryMethods** - Common discovery patterns
- **GeneratorConfiguration** - Configuration handling
- **AssemblyNameProvider** - Assembly name utilities