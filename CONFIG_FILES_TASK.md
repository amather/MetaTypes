# Multiple Generator Configuration: Research & Alternative Solutions

## Problem Statement

Currently, the MetaTypes generator system has a fundamental limitation: when multiple generator instances are used in a single project, they all share the same configuration file. This prevents scenarios where different generator instances (think: forked instances) need different configurations.

**Current Limitation:**
- All generators search for the first `AdditionalFiles` with `Type="MetaTypes.Generator.Options"`
- No way to specify different config files for different generator instances
- Multiple generator references all use identical configuration

**Use Cases Requiring This Feature:**
1. Multiple vendor-specific generators with different discovery methods
2. Different target namespaces for different generator instances
3. Separate diagnostic settings per generator instance
4. Different cross-assembly settings per generator

## Originally Proposed Solution (INVALID)

The initial approach proposed using MSBuild metadata to pass instance parameters:

```xml
<!-- INVALID APPROACH -->
<PropertyGroup>
  <GeneratorInstance>EfCore</GeneratorInstance>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="MetaTypes.Generator.csproj" ...>
    <GeneratorInstance>EfCore</GeneratorInstance>
  </ProjectReference>
</ItemGroup>
```

## Why the Original Approach is INVALID

After comprehensive research into MSBuild and Roslyn source generator architecture, this approach has **fundamental flaws**:

### 1. **MSBuild Properties are Project-Wide**
- `build_property.*` values are visible to ALL generators in the project
- No mechanism exists to pass different properties to different analyzer instances
- Multiple references to the same generator all see identical project-wide properties

### 2. **Source Generator Limitations**
- Confirmed by [Roslyn Issue #60595](https://github.com/dotnet/roslyn/issues/60595): Source generators cannot access arbitrary MSBuild item metadata
- `AnalyzerConfigOptionsProvider` only supports lookup against files with disk representation
- No "per-analyzer configuration" mechanism exists in the Roslyn architecture

### 3. **Multiple Generator Problem**
```xml
<!-- This fails - both generators see the same property -->
<PropertyGroup>
  <GeneratorInstance>EfCore</GeneratorInstance> <!-- ALL generators see this -->
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="MetaTypes.Generator.csproj" ... /> <!-- Sees EfCore -->
  <ProjectReference Include="MetaTypes.Generator.csproj" ... /> <!-- Also sees EfCore -->
</ItemGroup>
```

## Valid Alternative Solutions

### 1. **Configuration File Convention** (Recommended)

Modify `ConfigurationLoader` to support multiple configuration files through naming conventions:

```xml
<ItemGroup>
  <AdditionalFiles Include="metatypes.efcore.config.json" Type="MetaTypes.Generator.Options" />
  <AdditionalFiles Include="metatypes.statics.config.json" Type="MetaTypes.Generator.Options" />
</ItemGroup>
```

**Implementation Strategy:**
- Detect multiple config files in `ConfigurationLoader.LoadFromAdditionalFiles()`
- Use filename convention to determine specialization (`metatypes.{vendor}.config.json`)
- Auto-instantiate the generator internally for each configuration
- Merge configurations where appropriate

**Pros:**
- ✅ Works with current MSBuild/Roslyn limitations
- ✅ Minimal code changes to existing `ConfigurationLoader`
- ✅ Maintains backward compatibility
- ✅ True per-instance configuration

### 2. **Multi-Configuration Single File**

```json
{
  "Generators": {
    "EfCore": { 
      "Discovery": { "Methods": ["EfCore.TableAttribute"] },
      "Generation": { "BaseMetaTypes": true }
    },
    "Statics": { 
      "Discovery": { "Methods": ["Statics.ServiceMethod"] },
      "Generation": { "BaseMetaTypes": false }
    }
  }
}
```

**Implementation:**
- Extend `MetaTypesGeneratorConfiguration` to support multiple generator sections
- Modify `MetaTypeSourceGenerator` to iterate through configured generators
- Generate separate output for each generator section

### 3. **Separate Generator Packages**

```xml
<ItemGroup>
  <PackageReference Include="MetaTypes.Generator.EfCore" Version="1.0.0" />
  <PackageReference Include="MetaTypes.Generator.Statics" Version="1.0.0" />
</ItemGroup>
```

**Architecture:**
```
MetaTypes.Generator.Core         // Base functionality
MetaTypes.Generator.EfCore       // EfCore vendor
MetaTypes.Generator.Statics      // Statics vendor
```

**Pros:**
- ✅ Complete isolation between generators
- ✅ Independent versioning and updates
- ✅ Clear responsibility separation

**Cons:**
- ❌ More complex packaging and deployment
- ❌ Code duplication across packages
- ❌ Version coordination challenges

### 4. **Vendor-Agnostic Internal Routing**

Enhance the current vendor system to support internal generator multiplexing:

```json
{
  "MetaTypes.Generator": {
    "Instances": [
      {
        "Name": "EfCore",
        "Discovery": { "Methods": ["EfCore.TableAttribute"] },
        "Generation": { "BaseMetaTypes": true }
      },
      {
        "Name": "Statics", 
        "Discovery": { "Methods": ["Statics.ServiceMethod"] },
        "Generation": { "BaseMetaTypes": false }
      }
    ]
  }
}
```

## Implementation Plan for Recommended Solution

### Phase 1: Configuration File Convention

**Current ConfigurationLoader Logic:**
```csharp
// src/MetaTypes.Generator.Common/Configuration/ConfigurationLoader.cs:16-27
var configFile = additionalFiles.FirstOrDefault(file =>
{
    var options = configProvider.GetOptions(file);
    return options.TryGetValue("build_metadata.AdditionalFiles.Type", out var type) &&
           type == "MetaTypes.Generator.Options";
});
```

**Enhanced Logic:**
```csharp
public static IEnumerable<(string Instance, MetaTypesGeneratorConfiguration Config)> LoadAllConfigurations(
    ImmutableArray<AdditionalText> additionalFiles,
    AnalyzerConfigOptionsProvider configProvider)
{
    var configFiles = additionalFiles.Where(file =>
    {
        var options = configProvider.GetOptions(file);
        return options.TryGetValue("build_metadata.AdditionalFiles.Type", out var type) &&
               type == "MetaTypes.Generator.Options";
    });

    foreach (var configFile in configFiles)
    {
        var fileName = Path.GetFileNameWithoutExtension(configFile.Path);
        var instance = ExtractInstanceFromFileName(fileName); // "metatypes.efcore.config" -> "efcore"
        var config = ParseConfigurationFile(configFile);
        
        yield return (instance ?? "default", config);
    }
}

private static string? ExtractInstanceFromFileName(string fileName)
{
    // Pattern: metatypes.{instance}.config or metatypes.config
    var match = Regex.Match(fileName, @"^metatypes\.(.+)\.config$");
    return match.Success ? match.Groups[1].Value : null;
}
```

### Phase 2: Generator Modification

**Current Generator (src/MetaTypes.Generator/MetaTypeSourceGenerator.cs:21-29):**
```csharp
var configuration = context.AdditionalTextsProvider
    .Collect()
    .Combine(context.AnalyzerConfigOptionsProvider)
    .Select((combined, _) => 
    {
        var (additionalFiles, configProvider) = combined;
        var fullConfig = ConfigurationLoader.LoadFromAdditionalFiles(additionalFiles, configProvider);
        return fullConfig.BaseGenerator!;
    });
```

**Enhanced Generator:**
```csharp
var configurations = context.AdditionalTextsProvider
    .Collect()
    .Combine(context.AnalyzerConfigOptionsProvider)
    .Select((combined, _) => 
    {
        var (additionalFiles, configProvider) = combined;
        return ConfigurationLoader.LoadAllConfigurations(additionalFiles, configProvider).ToList();
    });

context.RegisterSourceOutput(compilationAndConfig,
    (spc, source) => 
    {
        var compilation = source.Left;
        var configs = source.Right;
        
        foreach (var (instance, config) in configs)
        {
            ExecuteForInstance(compilation, config, instance, spc);
        }
    });
```

## Success Criteria

- [ ] Multiple configuration files can be used in a single project
- [ ] Each configuration runs independently with its own settings
- [ ] Existing single-config projects continue working unchanged
- [ ] Generated files are properly namespaced per instance
- [ ] Performance impact is minimal
- [ ] Clear documentation and examples provided

## Timeline

- **Week 1**: Implement enhanced `ConfigurationLoader`
- **Week 2**: Modify `MetaTypeSourceGenerator` for multi-config support
- **Week 3**: Testing and validation
- **Week 4**: Documentation and samples

## Notes

- The original MSBuild-based approach is **architecturally impossible** with current Roslyn limitations
- File-based configuration convention is the most practical solution
- This approach aligns with existing MetaTypes architecture patterns
- Future Roslyn improvements might enable better per-analyzer configuration, but this solution works today