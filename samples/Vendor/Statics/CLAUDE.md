# Statics Vendor Sample

## Overview
This folder contains samples demonstrating the Statics vendor integration with MetaTypes. The Statics vendor discovers and generates metadata for static method collections, enabling compile-time service discovery patterns.

## Purpose
The Statics vendor allows you to:
- Discover static methods across your codebase
- Generate compile-time metadata for static service patterns
- Enable dependency injection for static method collections
- Organize static methods by their containing classes

## Sample Structure
```
Statics/
├── CLAUDE.md              # This file
└── [Future samples]       # Statics vendor samples will be added here
```

## Configuration Example

```json
{
  "MetaTypes.Generator": {
    "Generation": { "BaseMetaTypes": true },
    "Discovery": {
      "CrossAssembly": true,
      "Methods": ["Statics.StaticMethod"]
    },
    "EnabledVendors": ["Statics"],
    "VendorConfigs": {
      "Statics": {
        "RequireBaseTypes": true,
        "IncludePrivateMethods": false,
        "IncludeInternalMethods": true
      }
    }
  }
}
```

## Key Features (Planned)

✅ **Static Method Discovery** - Find static methods across assemblies  
✅ **Service Pattern Support** - Generate metadata for static service patterns  
✅ **DI Integration** - Service provider extensions for static method collections  
✅ **Cross-Assembly Support** - Works across multiple assemblies  

## Implementation Details

The Statics vendor will be implemented in:
- **Discovery**: `src/MetaTypes.Generator.Common/Vendor/Statics/Discovery/`
- **Generation**: `src/MetaTypes.Generator.Common/Vendor/Statics/Generation/`
- **Abstractions**: `src/MetaTypes.Abstractions/Vendor/Statics/`

## Status
🚧 **In Development** - Statics vendor samples are planned for future implementation.