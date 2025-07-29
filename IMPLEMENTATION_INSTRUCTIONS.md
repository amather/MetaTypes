# MetaTypes Source Generator Implementation Instructions

## Project Overview
Build a C# source generator that creates compile-time metadata for classes, structs, records, and enums to reduce reflection overhead at runtime. The generator produces `IMetaType` implementations with singleton patterns and compile-time constants.

## Solution Structure
Create a multi-project solution with the following layout:

```
MetaTypes/
├── src/
│   ├── MetaTypes.Abstractions/          # Core interfaces, attributes, and DI extensions
│   └── MetaTypes.Generator/             # Source generator implementation
├── samples/
│   ├── Sample.Business/                 # Sample model assembly 1
│   ├── Sample.Auth/                     # Sample model assembly 2
│   └── Sample.Console/                  # Consumer application
├── tests/
│   ├── MetaTypes.Generator.Tests/       # Generator unit tests
│   └── MetaTypes.Integration.Tests/     # End-to-end tests
└── docs/
    └── README.md
```

## Target Frameworks & Compatibility
- **MetaTypes.Abstractions**: `net8.0` (includes DI extensions)
- **MetaTypes.Generator**: `netstandard2.0` (source generator compatibility requirement)
- **Sample projects**: `net9.0` with C# 13 features
- **Test projects**: `net9.0`

## Core Components to Implement

### 1. MetaTypes.Abstractions
- `IMetaType` and `IMetaType<T>` interfaces with all specified properties and methods
- `IMetaTypeMember` interface with member metadata properties
- `IMetaTypeProvider` interface for assembly-level discovery
- `[MetaType]` attribute for marking types to process
- `AddMetaTypes<T>()` extension methods for `IServiceCollection`
- `GetMetaType<T>()` and `GetMetaType(Type)` extension methods for `IServiceProvider`
- Runtime type discovery and DI registration logic
- Use modern C# features where compatible (collection expressions, nullable reference types)

### 2. MetaTypes.Generator
- Incremental source generator using `IIncrementalGenerator`
- MSBuild property support for `<MetaTypeAssemblyName>` configuration
- Namespace extraction logic for common assembly prefixes
- Generate singleton-pattern implementations for all meta types
- Ensure generated code uses compile-time constants for optimal performance
- Handle generic types, nullable types, and attribute metadata
- Support expression-based member finding with proper generic constraints

## Coding Standards
- Use C# 13 features in sample/test projects (collection expressions, primary constructors, etc.)
- Apply nullable reference types throughout
- Follow .NET naming conventions and code style
- Use `sealed` classes for all generated implementations
- Implement lazy singleton pattern: `private static T? _instance; public static T Instance => _instance ??= new();`
- Prefer `IReadOnlyList<T>` over arrays for collections
- Use expression trees for compile-time member access

## Key Implementation Requirements
1. **Performance**: All generated properties should return compile-time constants
2. **Thread Safety**: Singleton implementations must be thread-safe
3. **Multi-Assembly Support**: Generator must work across multiple assemblies in same solution
4. **Attribute Handling**: Preserve and expose custom attributes on types and members
5. **Generic Type Support**: Handle generic types and their constraints properly
6. **Expression Support**: Enable lambda-based member discovery for type safety

## Sample Projects Structure
- Create at least two model assemblies with different namespaces
- Include various type patterns: classes, records, structs, generic types
- Demonstrate cross-assembly MetaType relationships
- Show DI integration in console application

## Testing Strategy
- Unit tests for source generator logic using `GeneratorDriver`
- Integration tests verifying generated code compiles and runs correctly
- Performance benchmarks comparing reflection vs MetaTypes approach
- Multi-assembly scenario testing

## Git Configuration
Create `.gitignore` excluding:
- `bin/`, `obj/` directories
- `.vs/`, `.vscode/` IDE folders
- `*.user` files
- `TestResults/`
- Package output directories

## Documentation Requirements
- README with quick start guide and examples
- API documentation for public interfaces
- Performance comparison metrics
- Contributing guidelines

Focus on creating a clean, extensible architecture that leverages modern C# features while maintaining compatibility requirements for the source generator component.