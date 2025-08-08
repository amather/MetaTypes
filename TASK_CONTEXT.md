# MetaTypes Project Context

## Current State (Post-LinkBase Implementation)

We successfully implemented the LinkBase approach for source generator code sharing, moving from manual file includes to automatic wildcard patterns. The project now has:

### Architecture
- **Real Projects**: MetaTypes.Abstractions, MetaTypes.Generator, MetaTypes.Generator.EfCore
- **Shared Code Folders**: MetaTypes.Generator.Common, MetaTypes.Generator.EfCore.Common (no .csproj files)
- **LinkBase Integration**: Automatic file inclusion via `**/*.cs` patterns with proper excludes

### Key Discoveries

1. **Code Sharing Necessity**: Source generators cannot use traditional project references or assembly sharing - LinkBase with wildcards is the solution

2. **Partial Class Architecture**: Base generator creates foundation classes with `Instance` properties, specialized generators extend them as partial implementations

3. **Generator Coordination Problem**: Multiple generators stepping on each other, creating duplicate base MetaTypes or missing dependencies

### Documentation Structure
- **README.md**: User guide ("How to use MetaTypes")
- **SHARED_CODE_USERS.md**: Developer guide ("How to build generators with MetaTypes shared code")
- **CLAUDE.md files**: Per-folder documentation explaining purpose and usage

### Recent Git History
```
dc6041e Remove outdated IMPLEMENTATION_INSTRUCTIONS.md
61fb848 A New Hope: implement LinkBase approach for source generator shared code
```

## The Problem That Led to This Task

As we learned about source generator code sharing, we realized the fundamental issue: **multiple MetaTypes-based generators trying to co-exist but conflicting with each other**.

Current issues:
- Each generator tries to be self-contained
- No coordination between generators  
- Duplicate base MetaTypes generation
- No way to configure discovery depth (syntax vs cross-assembly)
- No way to specify which generator should generate base types

## The Vision: Orchestrated Configuration System

A shared configuration format where:
1. Each generator finds its own section by name
2. Common configuration format (+ generator-specific extensions)
3. Configurable discovery depth and methods
4. Clear division of labor for base MetaTypes generation
5. Composable generator ecosystem

This transforms from "competing generators" to "coordinated generator orchestra".