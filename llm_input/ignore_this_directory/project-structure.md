# Project Structure and Configuration

## Solution Structure

```
MetaTypes/
├── src/
│   ├── MetaTypes.Core/
│   │   ├── MetaTypes.Core.csproj
│   │   ├── Attributes/
│   │   │   └── MetaTypeAttribute.cs
│   │   ├── Models/
│   │   │   ├── MetaType.cs
│   │   │   └── MetaTypeMember.cs
│   │   └── Interfaces/
│   │       ├── IMetaType.cs
│   │       └── IMetaTypeMember.cs
│   ├── MetaTypes.Generator/
│   │   ├── MetaTypes.Generator.csproj
│   │   ├── MetaTypeSourceGenerator.cs
│   │   ├── CodeGeneration/
│   │   │   ├── MetaTypeCodeGenerator.cs
│   │   │   └── MetaTypeMemberCodeGenerator.cs
│   │   ├── Analysis/
│   │   │   ├── TypeAnalyzer.cs
│   │   │   └── MemberAnalyzer.cs
│   │   └── Templates/
│   │       ├── MetaTypeTemplate.cs
│   │       └── MetaTypeMemberTemplate.cs
│   └── MetaTypes/
│       ├── MetaTypes.csproj
│       └── MetaTypes.cs (package reference aggregator)
├── tests/
│   ├── MetaTypes.Tests/
│   │   ├── MetaTypes.Tests.csproj
│   │   ├── SourceGeneratorTests.cs
│   │   ├── MetaTypeTests.cs
│   │   └── TestTypes/
│   │       ├── SimpleTestTypes.cs
│   │       ├── ComplexTestTypes.cs
│   │       └── GenericTestTypes.cs
│   └── MetaTypes.Integration.Tests/
│       ├── MetaTypes.Integration.Tests.csproj
│       └── EndToEndTests.cs
├── samples/
│   ├── BasicUsage/
│   │   ├── BasicUsage.csproj
│   │   └── Program.cs
│   └── AdvancedUsage/
│       ├── AdvancedUsage.csproj
│       └── Program.cs
├── docs/
│   ├── README.md
│   ├── getting-started.md
│   └── api-reference.md
├── build/
│   ├── Directory.Build.props
│   └── Directory.Build.targets
├── MetaTypes.sln
├── global.json
├── nuget.config
└── README.md
```

## Multi-Package Repository Structure

The solution produces three NuGet packages:

1. **MetaTypes.Core** - Base interfaces, attributes, and runtime models
2. **MetaTypes.Generator** - Source generator implementation (analyzer package)
3. **MetaTypes** - Meta-package that references both Core and Generator

## Project Configuration Files

### MetaTypes.Core.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes.Core</PackageId>
  </PropertyGroup>
</Project>
```

### MetaTypes.Generator.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes.Generator</PackageId>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="../MetaTypes.Core/MetaTypes.Core.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <None Include="tools/install.ps1" Pack="true" PackagePath="tools/install.ps1" />
    <None Include="tools/uninstall.ps1" Pack="true" PackagePath="tools/uninstall.ps1" />
  </ItemGroup>
  
  <ItemGroup>
    <Analyzer Include="$(OutputPath)/$(AssemblyName).dll" />
  </ItemGroup>
</Project>
```

### MetaTypes.csproj (meta-package)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>MetaTypes</PackageId>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="MetaTypes.Core" Version="$(Version)" />
    <PackageReference Include="MetaTypes.Generator" Version="$(Version)" />
  </ItemGroup>
</Project>
```

## Build Configuration

### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

### global.json
```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMajor"
  }
}
```

### .editorconfig
```ini
root = true

[*.cs]
# Use file-scoped namespaces
csharp_style_namespace_declarations = file_scoped
# Use collection expressions
dotnet_style_collection_initializer = true
# Use target-typed new
csharp_style_implicit_object_creation_when_type_is_apparent = true
# Use pattern matching
csharp_style_pattern_matching_over_is_with_cast_check = true
csharp_style_pattern_matching_over_as_with_null_check = true
# Use expression-bodied members
csharp_style_expression_bodied_methods = true
csharp_style_expression_bodied_properties = true
```

## Build Commands

### Prerequisites
- .NET 9 SDK
- C# 13 language features enabled

### Development Commands
```bash
# Restore packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack --configuration Release --output ./artifacts

# Build specific project
dotnet build src/MetaTypes.Core/

# Run source generator tests specifically
dotnet test tests/MetaTypes.Tests/ --filter Category=SourceGenerator

# Watch and rebuild on file changes
dotnet watch build

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate documentation
dotnet tool restore
dotnet tool run docfx docfx.json
```

## NuGet Package Metadata

```xml
<PropertyGroup>
  <PackageId>MetaTypes</PackageId>
  <Version>1.0.0</Version>
  <Authors>Your Name</Authors>
  <Description>Compile-time type metadata generation for .NET</Description>
  <PackageTags>sourcegenerator;reflection;metadata;compile-time</PackageTags>
  <PackageProjectUrl>https://github.com/yourorg/metatypes</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/yourorg/metatypes</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
</PropertyGroup>
```

## Target Framework Strategy

- **Target Framework**: 
  - .NET 9 for Core and Meta-package (leveraging modern features)
  - .NET Standard 2.0 for Generator (Roslyn compatibility requirement)
- **Language Version**: C# 13 for latest features
- **Nullable**: Enable for all projects
- **Warnings as Errors**: Enable for production builds
- **Deterministic Builds**: Enable for reproducible packages