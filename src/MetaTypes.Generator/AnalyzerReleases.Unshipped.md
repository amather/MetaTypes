; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MTSTAT0001 | MetaTypes.Statics | Error | StaticsRepositoryProvider on non-DbContext type
MTSTAT0002 | MetaTypes.Statics | Error | StaticsRepositoryIgnore on non-DbSet property
MTEFCORE0001 | MetaTypes.EfCore | Error | Type must inherit from DbContext
MTEFCORE0002 | MetaTypes.EfCore | Warning | DbContext has no DbSet properties
MTEFCORE0003 | MetaTypes.EfCore | Info | Entity missing MetaType attribute
MTEFCORE0100 | MetaTypes.EfCore | Warning | Entity missing key property
MTEFCORE0101 | MetaTypes.EfCore | Info | Entity has multiple key properties
