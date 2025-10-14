; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MTSTAT0001 | MetaTypes.Statics | Error | Invalid return type for StaticsServiceMethod
MTSTAT0002 | MetaTypes.Statics | Error | Route parameter missing from method signature
MTSTAT0003 | MetaTypes.Statics | Error | Methods with 'id' parameter must specify Entity
MTSTAT0004 | MetaTypes.Statics | Error | EntityGlobal with 'id' parameter is invalid
MTSTAT0005 | MetaTypes.Statics | Warning | Entity with 'id' should not be EntityGlobal
MTSTAT0006 | MetaTypes.Statics | Error | Route parameter type constraint does not match method parameter type
MTSTAT0007 | MetaTypes.Statics | Error | Path parameter is required
MTSTAT0100 | MetaTypes.Statics | Error | StaticsRepositoryProvider on non-DbContext type
MTSTAT0101 | MetaTypes.Statics | Warning | StaticsRepositoryIgnore on non-DbSet property
MTEFCORE0001 | MetaTypes.EfCore | Error | Type must inherit from DbContext
MTEFCORE0002 | MetaTypes.EfCore | Warning | DbContext has no DbSet properties
MTEFCORE0003 | MetaTypes.EfCore | Info | Entity missing MetaType attribute
MTEFCORE0100 | MetaTypes.EfCore | Warning | Entity missing key property
MTEFCORE0101 | MetaTypes.EfCore | Info | Entity has multiple key properties
