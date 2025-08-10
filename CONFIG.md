# MetaTypes Configuration Guide

> **✅ Updated August 10th, 2025** - All configuration examples have been verified with the current working architecture.

## Configuration File Setup

### 1. Create metatypes.config.json
Create a `metatypes.config.json` file in your project root with your desired configuration.

### 2. Configure Project File Visibility
In your `.csproj` file, add the configuration file with proper metadata for the source generator:

```xml
<ItemGroup>
  <!-- MetaTypes generator configuration (JSON) -->
  <AdditionalFiles Include="metatypes.config.json" Type="MetaTypes.Generator.Options" />
  <!-- Make configuration file metadata visible to source generators -->
  <CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="Type" />
</ItemGroup>
```

**Important**: Both the `Type="MetaTypes.Generator.Options"` metadata and the `CompilerVisibleItemMetadata` are required for the generator to find and load your configuration.

## Configuration Format

The configuration uses the following structure:

```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    },
    "Vendors": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```

## Configuration Options

### Base Generator Options

#### Generation Settings
- **`BaseMetaTypes`** (bool): Whether to generate base MetaType classes. Set to `true` for primary projects, `false` for vendor-only projects.

#### Discovery Settings
- **`Syntax`** (bool): Enable syntax tree analysis for type discovery
- **`CrossAssembly`** (bool): Enable cross-assembly reference scanning
- **`Methods`** (string[]): Array of discovery method identifiers

### Available Discovery Methods

#### Base Methods
- **`"MetaTypes.Attribute"`**: Discovers types marked with `[MetaType]` attribute
- **`"MetaTypes.Reference"`**: Discovers referenced types that have `[MetaType]` attribute

#### EfCore Vendor Methods
- **`"EfCore.TableAttribute"`**: Discovers EF Core entities with `[Table]` attribute via syntax analysis
- **`"EfCore.DbContextSet"`**: Discovers entity types by scanning `DbContext` `DbSet<T>` properties

### Vendor Configuration

#### EfCore Vendor Options
- **`RequireBaseTypes`** (bool): Whether EfCore extensions require base MetaTypes to be generated first
- **`IncludeNavigationProperties`** (bool): Generate navigation property metadata
- **`IncludeForeignKeys`** (bool): Generate foreign key relationship metadata

## Example Configurations

### Basic Project (Generate Base Types Only)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    }
  }
}
```

### EfCore Project (With Vendor Extensions)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": [
        "MetaTypes.Attribute", 
        "MetaTypes.Reference",
        "EfCore.TableAttribute",
        "EfCore.DbContextSet"
      ]
    },
    "Vendors": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```

### Infrastructure Project (No Base Types)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": false
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": false,
      "Methods": ["MetaTypes.Attribute"]
    }
  }
}
```

## Debugging Configuration

Set `"EnableDiagnosticFiles": true` to generate diagnostic files showing:
- Configuration loading status
- Discovery method execution results
- Discovery breakdown by method
- Generated type summary with discovery sources

**Diagnostic File Location:**
```
obj/Debug/net{version}/generated/MetaTypes.Generator/MetaTypes.Generator.MetaTypeSourceGenerator/_MetaTypesGeneratorDiagnostic.g.cs
```

**Sample Diagnostic Output:**
```csharp
// DISCOVERY EXECUTION:
// - Success: True
// - Methods Used: MetaTypes.Attribute, EfCore.TableAttribute
// - Warnings: 
// - Errors: 
//
// DISCOVERY RESULTS:  
// - Discovered types: 3
// - Discovery breakdown: Syntax-MetaTypes.Attribute: 2, Syntax-EfCore.TableAttribute: 1
// - Types: Customer (Syntax by [MetaTypes.Attribute]), Order (Syntax by [MetaTypes.Attribute, EfCore.TableAttribute])
```

## Working Examples

**Verified working configurations from sample projects:**

### Basic Project (Sample.Console)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": ["MetaTypes.Attribute", "MetaTypes.Reference"]
    }
  }
}
```

### EfCore Project (Sample.EfCore.LocalOnly)
```json
{
  "MetaTypes.Generator": {
    "EnableDiagnosticFiles": true,
    "Generation": {
      "BaseMetaTypes": true
    },
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": true,
      "Methods": [
        "MetaTypes.Attribute", 
        "MetaTypes.Reference",
        "EfCore.TableAttribute",
        "EfCore.DbContextSet"
      ]
    },
    "Vendors": {
      "EfCore": {
        "RequireBaseTypes": true,
        "IncludeNavigationProperties": true,
        "IncludeForeignKeys": true
      }
    }
  }
}
```