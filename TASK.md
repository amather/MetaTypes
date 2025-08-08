# TASK: Implement Orchestrated Generator Configuration System

> **Context**: Read [TASK_CONTEXT.md](./TASK_CONTEXT.md) and relevant [CLAUDE.md files](./src/) for full background.

## 🎯 Goal
Transform MetaTypes from competing generators to an orchestrated system where multiple generators can co-exist through shared configuration.

## 📋 Implementation Tasks

### Phase 1: Configuration Infrastructure
- [ ] **1.1** Design `IGeneratorConfiguration` interface for shared config format
- [ ] **1.2** Create configuration model classes:
  - [ ] `GeneratorConfigSection` (base section format)  
  - [ ] `DiscoveryConfig` (syntax/cross-assembly + methods)
  - [ ] `GenerationConfig` (BaseMetaTypes flag)
- [ ] **1.3** Implement configuration loader that reads section by generator name
- [ ] **1.4** Add support for generator-specific extensions (e.g., EfCore section)
- [ ] **1.5** Update existing configuration loading in both generators

### Phase 2: Discovery System Refactoring
- [ ] **2.1** Separate type discovery from generation logic
- [ ] **2.2** Make discovery methods configurable via boolean flags:
  - [ ] `MetaTypesAttributes` (default: true)
  - [ ] `MetaTypesReferences` (default: true) 
  - [ ] `EfCoreEntities` (EfCore-specific)
  - [ ] `DbContextScanning` (EfCore-specific)
- [ ] **2.3** Implement `Syntax` and `CrossAssembly` discovery depth options
- [ ] **2.4** Update `UnifiedTypeDiscovery` to respect configuration flags
- [ ] **2.5** Update `EfCoreDiscoveryMethods` to be configurable

### Phase 3: Generation Coordination
- [ ] **3.1** Implement `BaseMetaTypes` flag (default: false)
- [ ] **3.2** Update base generator to conditionally skip base generation
- [ ] **3.3** Update EfCore generator to verify base types exist when needed
- [ ] **3.4** Add configuration validation (ensure at least one generator does base generation)
- [ ] **3.5** Improve error messages when base types missing

### Phase 4: Configuration Format & Schema
- [ ] **4.1** Design JSON configuration schema:
```json
{
  "AssemblyName": "MyApp.Business",
  "GeneratorName": {
    "Discovery": {
      "Syntax": true,
      "CrossAssembly": false,
      "Methods": {
        "MetaTypesAttributes": true,
        "MetaTypesReferences": true
      }
    },
    "Generation": {
      "BaseMetaTypes": false
    }
  }
}
```
- [ ] **4.2** Add MSBuild property support as fallback
- [ ] **4.3** Create configuration examples and documentation
- [ ] **4.4** Update SHARED_CODE_USERS.md with configuration guide

### Phase 5: Sample Project Reconfiguration
- [ ] **5.1** Reconfigure sample projects to follow MetaTypes approach:
  - [ ] Move generators from library projects to main consumer project (Sample.Console)
  - [ ] Configure Sample.Console to detect/generate for all referenced libraries
  - [ ] Update metatypes.config.json to coordinate multiple generators
  - [ ] Ensure cross-assembly discovery works properly
- [ ] **5.2** Update sample projects with new configuration format
- [ ] **5.3** Test generator coordination scenarios:
  - [ ] Base generator only
  - [ ] EfCore generator only (should fail gracefully)
  - [ ] Both generators with proper coordination
  - [ ] Multiple discovery methods enabled
- [ ] **5.4** Update diagnostics to show configuration decisions
- [ ] **5.5** Verify LinkBase approach still works with new architecture

### Phase 6: Documentation & Examples
- [ ] **6.1** Update README.md with new configuration approach
- [ ] **6.2** Create configuration examples for common scenarios
- [ ] **6.3** Update CLAUDE.md files with new architecture details
- [ ] **6.4** Document generator coordination patterns

## 🔄 Restart Instructions

To resume this task after interruption:
1. Read `TASK_CONTEXT.md` for background
2. Check current phase progress above
3. Review `CLAUDE.md` files in affected folders
4. Continue with next unchecked item

## 🎯 Success Criteria

✅ Multiple MetaTypes generators can co-exist without conflicts  
✅ Configuration clearly defines discovery and generation responsibilities  
✅ Generators can be composed (Base + EfCore + Custom)  
✅ Shared configuration format with generator-specific extensions  
✅ **Sample projects compile and run successfully**  
✅ **Sample.Console generates MetaTypes for all referenced libraries (Sample.Business, Sample.Auth)**  
✅ **Cross-assembly discovery works from main consumer project**  
✅ Clear error messages when configuration is invalid  
✅ Backwards compatibility path documented (if not maintained)  

## 🚨 Breaking Changes Expected

- Configuration format changes (will need migration guide)
- Default `BaseMetaTypes: false` (most generators won't generate base types by default)
- Discovery method configuration (previously always-on methods now configurable)

---
*Generated during MetaTypes architecture evolution - maintaining the Force strong in this codebase.*