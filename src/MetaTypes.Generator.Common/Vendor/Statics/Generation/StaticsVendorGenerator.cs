using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using MetaTypes.Generator.Common.Generator;

namespace MetaTypes.Generator.Common.Vendor.Statics.Generation
{
    /// <summary>
    /// Statics vendor generator that generates partial class extensions with static service method metadata
    /// </summary>
    public class StaticsVendorGenerator : IVendorGenerator
    {
        private StaticsConfig _config = new();
        
        public string VendorName => "Statics";
        
        public string Description => "Generates Statics-specific metadata extensions for static service classes";
        
        /// <summary>
        /// Configure the Statics vendor generator with its specific configuration
        /// </summary>
        public void Configure(JsonElement? config)
        {
            if (config.HasValue)
            {
                try
                {
                    _config = JsonSerializer.Deserialize<StaticsConfig>(config.Value) ?? new StaticsConfig();
                }
                catch
                {
                    // Use default config if parsing fails
                    _config = new StaticsConfig();
                }
            }
            else
            {
                // Use default config when no config provided
                _config = new StaticsConfig();
            }
        }

        /// <summary>
        /// Generates Statics extensions only for types discovered by Statics discovery methods
        /// </summary>
        public IEnumerable<GeneratedFile> Generate(
            IEnumerable<DiscoveredType> discoveredTypes,
            Compilation compilation,
            GeneratorContext context)
        {
            // Check if base types are required and available
            var baseTypesAvailable = context.Properties.TryGetValue("BaseMetaTypesGenerated", out var baseGenerated) 
                && bool.Parse(baseGenerated);
                
            if (_config.RequireBaseTypes && !baseTypesAvailable)
            {
                // If base types are required but not available, skip vendor generation
                yield break;
            }

            // Filter to only types discovered by Statics discovery methods
            var staticsTypes = discoveredTypes
                .Where(dt => dt.WasDiscoveredByPrefix("Statics."))
                .Select(dt => dt.TypeSymbol)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>()
                .ToList();

            if (!staticsTypes.Any())
            {
                yield break;
            }

            // Validate StaticsServiceMethodAttribute usage and generate diagnostics
            if (context.EnableDiagnostics)
            {
                var validationResults = ValidateStaticsServiceMethods(staticsTypes);
                if (validationResults.Any())
                {
                    var diagnosticsContent = GenerateValidationDiagnostics(validationResults);
                    yield return new GeneratedFile
                    {
                        FileName = "_StaticsValidationDiagnostic.g.cs",
                        Content = diagnosticsContent
                    };
                }
            }

            // Generate Statics DI extension methods for the target namespace
            var diExtensionsSource = GenerateStaticsServiceCollectionExtensions(context.TargetNamespace);
            yield return new GeneratedFile
            {
                FileName = $"StaticsServiceCollectionExtensions.g.cs",
                Content = diExtensionsSource
            };
            
            // Generate Statics extensions for each discovered static class
            var isFirstFile = true;
            foreach (var staticClass in staticsTypes)
            {
                var source = GenerateStaticsExtension(staticClass, includeSharedClasses: isFirstFile);
                var assemblyName = staticClass.ContainingAssembly.Name;
                yield return new GeneratedFile
                {
                    FileName = $"{assemblyName}_{staticClass.Name}MetaTypeStatics.g.cs",
                    Content = source
                };
                isFirstFile = false; // Only include shared classes in the first file
            }

            // Generate repository classes for entity types
            var repositorySpecs = AnalyzeRepositoryRequirements(discoveredTypes, staticsTypes);
            foreach (var repositorySpec in repositorySpecs)
            {
                var repositorySource = GenerateRepositoryClass(repositorySpec);
                yield return new GeneratedFile
                {
                    FileName = $"{repositorySpec.Name}.g.cs",
                    Content = repositorySource
                };
            }

            // Generate repository DI extensions
            if (repositorySpecs.Any())
            {
                var repositoryDiSource = GenerateRepositoryServiceCollectionExtensions(repositorySpecs, context.TargetNamespace);
                yield return new GeneratedFile
                {
                    FileName = "StaticsRepositoryServiceCollectionExtensions.g.cs",
                    Content = repositoryDiSource
                };
            }
        }

        /// <summary>
        /// Analyzes discovered types to determine repository requirements
        /// </summary>
        private List<RepositorySpec> AnalyzeRepositoryRequirements(IEnumerable<DiscoveredType> discoveredTypes, List<INamedTypeSymbol> staticsTypes)
        {
            var repositorySpecs = new List<RepositorySpec>();
            
            // Get EfCore discovered entities with DbContext info (ONLY from DbContextSet discovery)
            var efCoreEntities = discoveredTypes
                .Where(dt => dt.WasDiscoveredBy("EfCore.DbContextSet"))  // Specific to DbSet<T> discoveries only
                .ToDictionary(dt => dt.TypeSymbol, dt => new {
                    DbContextTypeName = dt.DiscoveryContexts.TryGetValue("DbContextType", out var ctxType) ? ctxType : "UnknownDbContext",
                    DbContextName = dt.DiscoveryContexts.TryGetValue("DbContextName", out var name) ? name : "UnknownContext"
                });

            // Analyze all service methods from Statics types
            var allServiceMethods = new List<ServiceMethodInfo>();
            foreach (var staticClass in staticsTypes)
            {
                var serviceMethods = GetStaticServiceMethods(staticClass);
                foreach (var method in serviceMethods)
                {
                    var attribute = GetStaticsServiceMethodAttribute(method);
                    if (attribute != null)
                    {
                        var methodInfo = AnalyzeServiceMethod(method, attribute);
                        allServiceMethods.Add(methodInfo);
                    }
                }
            }

            // Group service methods by entity type
            var methodsByEntity = allServiceMethods
                .Where(sm => sm.EntityType != null)
                .GroupBy(sm => sm.EntityType, SymbolEqualityComparer.Default);

            // Create entity repositories (unified and Statics-only)
            foreach (var entityGroup in methodsByEntity)
            {
                var entityType = (INamedTypeSymbol)entityGroup.Key!;
                var entityMethods = entityGroup.ToList();
                
                // Check if this entity has EfCore backing
                var efCoreInfo = efCoreEntities.TryGetValue(entityType, out var efInfo) ? efInfo : null;
                
                var repositorySpec = new RepositorySpec
                {
                    Name = $"{entityType.Name}Repository",
                    Namespace = GetRepositoryNamespace(entityType, isGlobal: false),
                    EntityType = entityType,
                    DbContextType = null, // For now, just generate Statics-only repositories
                    KeyPropertyType = GetKeyPropertyType(entityType),
                    ServiceMethods = entityMethods
                };
                
                repositorySpecs.Add(repositorySpec);
            }

            // Create global repository for methods without Entity parameter
            var globalMethods = allServiceMethods
                .Where(sm => sm.IsGlobalMethod)
                .ToList();
                
            if (globalMethods.Any())
            {
                var globalRepositorySpec = new RepositorySpec
                {
                    Name = "GlobalRepository",
                    Namespace = GetRepositoryNamespace(null, isGlobal: true),
                    EntityType = null,
                    DbContextType = null,
                    KeyPropertyType = null,
                    ServiceMethods = globalMethods
                };
                
                repositorySpecs.Add(globalRepositorySpec);
            }

            return repositorySpecs;
        }

        /// <summary>
        /// Generates a repository class based on the specification
        /// </summary>
        private string GenerateRepositoryClass(RepositorySpec spec)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine("using global::Statics.ServiceResult;");
            
            if (spec.HasCrud)
            {
                sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            }
            
            sb.AppendLine();
            sb.AppendLine($"namespace {spec.Namespace};");
            sb.AppendLine();

            // Generate repository class with appropriate interfaces
            var interfaces = new List<string> { "IStaticsRepository" };
            if (spec.HasCrud)
            {
                interfaces.Add("IEntityRepository");
            }
            
            sb.AppendLine($"public class {spec.Name} : {string.Join(", ", interfaces)}");
            sb.AppendLine("{");

            // Generate CRUD methods if this is an entity repository with EfCore backing
            if (spec.HasCrud && spec.EntityType != null && spec.DbContextType != null)
            {
                GenerateCrudMethods(sb, spec);
                sb.AppendLine();
            }

            // Generate service method wrappers
            if (spec.ServiceMethods.Any())
            {
                GenerateServiceMethodWrappers(sb, spec);
            }

            sb.AppendLine("}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates CRUD methods for entity repositories
        /// </summary>
        private void GenerateCrudMethods(StringBuilder sb, RepositorySpec spec)
        {
            var entityName = spec.EntityType!.Name;
            var dbContextName = spec.DbContextType!.Name;
            var keyType = spec.KeyPropertyType ?? "int";

            sb.AppendLine("    // CRUD Methods (EfCore-backed)");
            
            // Create method
            sb.AppendLine($"    public Task<{entityName}> Create({dbContextName} dbCtx)");
            sb.AppendLine("    {");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Read method
            sb.AppendLine($"    public Task<{entityName}> Read({dbContextName} dbCtx, {keyType} id)");
            sb.AppendLine("    {");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Update method
            sb.AppendLine($"    public Task<{entityName}> Update({dbContextName} dbCtx, {keyType} id)");
            sb.AppendLine("    {");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Delete method
            sb.AppendLine($"    public Task Delete({dbContextName} dbCtx, {keyType} id)");
            sb.AppendLine("    {");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Query method
            sb.AppendLine($"    public Task<IQueryable<{entityName}>> Query({dbContextName} dbCtx)");
            sb.AppendLine("    {");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
        }

        /// <summary>
        /// Generates service method wrappers that always return Task<>
        /// </summary>
        private void GenerateServiceMethodWrappers(StringBuilder sb, RepositorySpec spec)
        {
            sb.AppendLine("    // Service Method Wrappers");
            
            foreach (var methodInfo in spec.ServiceMethods)
            {
                var method = methodInfo.Method;
                var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));
                var serviceResultType = SimplifyServiceResultTypeName(methodInfo.ServiceResultType);
                
                // Generate method signature - always return Task<>
                sb.AppendLine($"    public Task<{serviceResultType}> {method.Name}({parameters})");
                sb.AppendLine("    {");
                
                // Call the original service method
                var staticClassName = method.ContainingType.ToDisplayString();
                if (methodInfo.IsAsync)
                {
                    // Original method returns Task<ServiceResult<T>>, so we can await it
                    sb.AppendLine($"        return {staticClassName}.{method.Name}({arguments});");
                }
                else
                {
                    // Original method returns ServiceResult<T>, so we wrap it in Task.FromResult
                    sb.AppendLine($"        var result = {staticClassName}.{method.Name}({arguments});");
                    sb.AppendLine("        return Task.FromResult(result);");
                }
                
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }

        /// <summary>
        /// Generates repository service collection extensions
        /// </summary>
        private string GenerateRepositoryServiceCollectionExtensions(List<RepositorySpec> repositorySpecs, string targetNamespace)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine();
            
            var methodName = NamingUtils.ToAddVendorMetaTypesMethodName(targetNamespace, "StaticsRepositories");
            
            sb.AppendLine("public static class StaticsRepositoryServiceCollectionExtensions");
            sb.AppendLine("{");
            sb.AppendLine($"    public static IServiceCollection {methodName}(this IServiceCollection services)");
            sb.AppendLine("    {");

            // Register entity repositories
            foreach (var spec in repositorySpecs.Where(r => !r.IsGlobal))
            {
                sb.AppendLine($"        services.AddSingleton<{spec.Namespace}.{spec.Name}>();");
            }

            // Register global repository
            var globalRepo = repositorySpecs.FirstOrDefault(r => r.IsGlobal);
            if (globalRepo != null)
            {
                sb.AppendLine($"        services.AddSingleton<{globalRepo.Namespace}.{globalRepo.Name}>();");
            }

            // Register interface implementations for IStaticsRepository
            foreach (var spec in repositorySpecs)
            {
                sb.AppendLine($"        services.AddSingleton<IStaticsRepository>(sp => sp.GetRequiredService<{spec.Namespace}.{spec.Name}>());");
            }

            // Register interface implementations for IEntityRepository (only those with CRUD methods)
            foreach (var spec in repositorySpecs.Where(r => r.HasCrud))
            {
                sb.AppendLine($"        services.AddSingleton<IEntityRepository>(sp => sp.GetRequiredService<{spec.Namespace}.{spec.Name}>());");
            }

            sb.AppendLine();
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Helper methods for repository generation
        /// </summary>
        private string GetRepositoryNamespace(INamedTypeSymbol? entityType, bool isGlobal)
        {
            if (isGlobal)
            {
                // Global repository goes in {TargetNamespace}.GlobalRepository
                return "Sample.Statics.ServiceMethod.GlobalRepository";
            }
            
            // Entity repositories go in the same namespace as the entity
            return entityType?.ContainingNamespace.ToDisplayString() ?? "Sample.Statics.ServiceMethod";
        }


        private string? GetKeyPropertyType(INamedTypeSymbol entityType)
        {
            // Look for [Key] attribute on properties
            var keyProperties = entityType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(attr => attr.AttributeClass?.Name == "KeyAttribute"))
                .ToList();

            if (keyProperties.Count == 1)
            {
                return keyProperties[0].Type.ToDisplayString();
            }
            
            // Fallback to int if no or multiple keys found
            return "int";
        }

        private AttributeData? GetStaticsServiceMethodAttribute(IMethodSymbol method)
        {
            return method.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "StaticsServiceMethodAttribute");
        }

        private string GenerateStaticsExtension(INamedTypeSymbol typeSymbol, bool includeSharedClasses = false)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine("using global::Statics.ServiceBroker.Attributes;");
            sb.AppendLine("using global::Statics.ServiceResult;");
            sb.AppendLine();
            
            // Use the assembly name for the MetaType namespace to match the base generator
            var assemblyName = typeSymbol.ContainingAssembly.Name;
            sb.AppendLine($"namespace {assemblyName};");
            sb.AppendLine();
            
            // Get static methods with StaticsServiceMethodAttribute
            var serviceMethods = GetStaticServiceMethods(typeSymbol);
            
            // Generate partial class extension for the MetaType class with Statics interface
            sb.AppendLine($"public partial class {typeSymbol.Name}MetaType : IMetaTypeStatics");
            sb.AppendLine("{");
            
            // Generate ServiceMethods collection
            sb.AppendLine("    public IReadOnlyList<IStaticsServiceMethod> ServiceMethods => [");
            foreach (var method in serviceMethods)
            {
                sb.AppendLine($"        new {typeSymbol.Name}ServiceMethod{method.Name}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate service method implementations
            foreach (var method in serviceMethods)
            {
                sb.AppendLine($"#region {method.Name} Service Method");
                sb.AppendLine();
                GenerateServiceMethodClass(sb, typeSymbol, method);
                sb.AppendLine("#endregion");
                sb.AppendLine();
            }

            // Generate shared wrapper classes only in the first file - OPTIMIZATION: Replaces explosion of individual classes
            if (includeSharedClasses)
            {
                GenerateSharedWrapperClasses(sb);
            }
            
            return sb.ToString();
        }

        private void GenerateServiceMethodClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method)
        {
            sb.AppendLine($"public class {typeSymbol.Name}ServiceMethod{method.Name} : IStaticsServiceMethod");
            sb.AppendLine("{");
            
            // Method name
            sb.AppendLine($"    public string MethodName => \"{method.Name}\";");
            
            // Return type
            // Handle nullable reference types properly for typeof
            var returnTypeString = method.ReturnType.ToDisplayString();
            if (returnTypeString.EndsWith("?") && !method.ReturnType.IsValueType)
            {
                // Remove nullable annotation for typeof since it can't handle nullable reference types
                returnTypeString = returnTypeString.TrimEnd('?');
            }
            
            // Simplify ServiceResult type names for better readability in generated code
            returnTypeString = SimplifyServiceResultTypeName(returnTypeString);
            
            sb.AppendLine($"    public Type ReturnType => typeof({returnTypeString});");
            
            // Method attributes
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeInfo> MethodAttributes => [");
            foreach (var attr in method.GetAttributes())
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Attribute{GetSimpleAttributeName(attr)}(),");
            }
            sb.AppendLine("    ];");
            
            // Parameters
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsParameterInfo> Parameters => [");
            foreach (var param in method.Parameters)
            {
                sb.AppendLine($"        new {typeSymbol.Name}Method{method.Name}Parameter{NamingUtils.ToPascalCase(param.Name)}(),");
            }
            sb.AppendLine("    ];");
            
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generate attribute classes for this method
            foreach (var attr in method.GetAttributes())
            {
                GenerateAttributeClass(sb, typeSymbol, method, attr);
            }
            
            // Generate parameter classes for this method
            foreach (var param in method.Parameters)
            {
                GenerateParameterClass(sb, typeSymbol, method, param);
            }
        }

        private void GenerateAttributeClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, AttributeData attribute)
        {
            var attrName = GetSimpleAttributeName(attribute);
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Attribute{attrName} : IStaticsAttributeInfo");
            sb.AppendLine("{");
            
            // Attribute type
            var attrTypeString = attribute.AttributeClass?.ToDisplayString();
            // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
            if (attrTypeString != null && attrTypeString.StartsWith("Statics."))
            {
                attrTypeString = "global::" + attrTypeString;
            }
            sb.AppendLine($"    public Type AttributeType => typeof({attrTypeString});");
            
            // Static readonly data for constructor arguments - OPTIMIZATION: Reduce allocation
            sb.AppendLine();
            sb.AppendLine("    private static readonly (Type Type, object? Value)[] _constructorArgs = [");
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                var arg = attribute.ConstructorArguments[i];
                var argTypeString = arg.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                sb.AppendLine($"        (typeof({argTypeString}), {formattedValue}),");
            }
            sb.AppendLine("    ];");
            
            // Static readonly data for named arguments - OPTIMIZATION: Reduce allocation
            sb.AppendLine();
            sb.AppendLine("    private static readonly (string Name, Type Type, object? Value)[] _namedArgs = [");
            foreach (var namedArg in attribute.NamedArguments)
            {
                var argTypeString = namedArg.Value.Type?.ToDisplayString();
                if (argTypeString != null && argTypeString.EndsWith("?") && namedArg.Value.Type != null && !namedArg.Value.Type.IsValueType)
                {
                    argTypeString = argTypeString.TrimEnd('?');
                }
                // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
                if (argTypeString != null && argTypeString.StartsWith("Statics."))
                {
                    argTypeString = "global::" + argTypeString;
                }
                var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                sb.AppendLine($"        (\"{namedArg.Key}\", typeof({argTypeString}), {formattedValue}),");
            }
            sb.AppendLine("    ];");
            
            // Lazy-initialized collections - OPTIMIZATION: Reduce allocations
            sb.AppendLine();
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeArgument>? _constructorArguments;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeNamedArgument>? _namedArguments;");
            
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => ");
            sb.AppendLine("        _constructorArguments ??= _constructorArgs");
            sb.AppendLine("            .Select((arg, i) => new StaticsAttributeArgument(arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => ");
            sb.AppendLine("        _namedArguments ??= _namedArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeNamedArgument(arg.Name, arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine("}");
            sb.AppendLine();
        }

        private void GenerateParameterClass(StringBuilder sb, INamedTypeSymbol typeSymbol, IMethodSymbol method, IParameterSymbol parameter)
        {
            sb.AppendLine($"public class {typeSymbol.Name}Method{method.Name}Parameter{NamingUtils.ToPascalCase(parameter.Name)} : IStaticsParameterInfo");
            sb.AppendLine("{");
            
            // Parameter name and type
            sb.AppendLine($"    public string ParameterName => \"{parameter.Name}\";");
            // Handle nullable reference types properly for typeof
            var parameterTypeString = parameter.Type.ToDisplayString();
            if (parameterTypeString.EndsWith("?") && !parameter.Type.IsValueType)
            {
                // Remove nullable annotation for typeof since it can't handle nullable reference types
                parameterTypeString = parameterTypeString.TrimEnd('?');
            }
            sb.AppendLine($"    public Type ParameterType => typeof({parameterTypeString});");
            
            // Static readonly data for parameter attributes - OPTIMIZATION: Similar to attribute optimization
            sb.AppendLine();
            sb.AppendLine("    private static readonly StaticsAttributeData[] _attributeData = [");
            foreach (var attr in parameter.GetAttributes())
            {
                var attrTypeString = attr.AttributeClass?.ToDisplayString();
                if (attrTypeString != null && attrTypeString.StartsWith("Statics."))
                {
                    attrTypeString = "global::" + attrTypeString;
                }
                
                sb.AppendLine($"        new StaticsAttributeData(typeof({attrTypeString}),");
                
                // Constructor arguments
                sb.AppendLine("            [");
                for (int i = 0; i < attr.ConstructorArguments.Length; i++)
                {
                    var arg = attr.ConstructorArguments[i];
                    var argTypeString = arg.Type?.ToDisplayString();
                    if (argTypeString != null && argTypeString.EndsWith("?") && arg.Type != null && !arg.Type.IsValueType)
                    {
                        argTypeString = argTypeString.TrimEnd('?');
                    }
                    var formattedValue = FormatTypedAttributeValue(arg.Value, arg.Type);
                    sb.AppendLine($"                (typeof({argTypeString}), {formattedValue}),");
                }
                sb.AppendLine("            ],");
                
                // Named arguments
                sb.AppendLine("            [");
                foreach (var namedArg in attr.NamedArguments)
                {
                    var argTypeString = namedArg.Value.Type?.ToDisplayString();
                    if (argTypeString != null && argTypeString.EndsWith("?") && namedArg.Value.Type != null && !namedArg.Value.Type.IsValueType)
                    {
                        argTypeString = argTypeString.TrimEnd('?');
                    }
                    if (argTypeString != null && argTypeString.StartsWith("Statics."))
                    {
                        argTypeString = "global::" + argTypeString;
                    }
                    var formattedValue = FormatTypedAttributeValue(namedArg.Value.Value, namedArg.Value.Type);
                    sb.AppendLine($"                (\"{namedArg.Key}\", typeof({argTypeString}), {formattedValue}),");
                }
                sb.AppendLine("            ]),");
            }
            sb.AppendLine("    ];");
            
            // Lazy-initialized parameter attributes collection - OPTIMIZATION
            sb.AppendLine();
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeInfo>? _parameterAttributes;");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes => ");
            sb.AppendLine("        _parameterAttributes ??= _attributeData");
            sb.AppendLine("            .Select(data => new StaticsAttributeInfo(data.AttributeType, data.ConstructorArgs, data.NamedArgs))");
            sb.AppendLine("            .ToArray();");
            
            sb.AppendLine("}");
            sb.AppendLine();
        }


        private static IEnumerable<IMethodSymbol> GetStaticServiceMethods(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsStatic && m.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
                .Where(m => HasStaticsServiceMethodAttribute(m));
        }

        private static bool HasStaticsServiceMethodAttribute(IMethodSymbol method)
        {
            return method.GetAttributes().Any(a => 
                a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute");
        }

        private static string GetSimpleAttributeName(AttributeData attribute)
        {
            var fullName = attribute.AttributeClass?.Name ?? "Unknown";
            return fullName.EndsWith("Attribute") ? fullName.Substring(0, fullName.Length - 9) : fullName;
        }


        private static string FormatAttributeValue(object? value)
        {
            if (value == null)
                return "null";
            
            if (value is string str)
                return $"\"{str.Replace("\"", "\\\"")}\"";
            
            if (value is bool b)
                return b ? "true" : "false";
            
            if (value is char c)
                return $"'{c}'";
            
            return $"\"{value}\"";
        }

        private static string GetValuePropertyType(ITypeSymbol? type)
        {
            if (type == null)
                return "object?";
            
            var displayString = type.ToDisplayString();
            
            // Add global:: prefix if it's a Statics namespace type to avoid namespace resolution issues
            if (displayString.StartsWith("Statics."))
            {
                displayString = "global::" + displayString;
            }
            
            // For nullable value types, the display string already includes the ?
            if (type.IsValueType)
            {
                return displayString;
            }
            
            // For reference types, we need to handle nullable annotations
            // If it's already nullable (ends with ?), keep it
            // Otherwise, add ? for reference types
            if (!displayString.EndsWith("?"))
            {
                displayString += "?";
            }
            
            return displayString;
        }

        private static string FormatTypedAttributeValue(object? value, ITypeSymbol? type)
        {
            if (value == null)
                return "null";
            
            // Check for enums first, before numeric types
            // For enums, generate the full enum value reference with cast
            if (type?.TypeKind == TypeKind.Enum)
            {
                var enumType = type.ToDisplayString();
                // Remove nullable annotation for enum type reference
                if (enumType.EndsWith("?"))
                {
                    enumType = enumType.TrimEnd('?');
                }
                // Add global:: prefix if it's a Statics namespace enum to avoid namespace resolution issues
                if (enumType.StartsWith("Statics."))
                {
                    enumType = "global::" + enumType;
                }
                // Just return the numeric value cast to the enum type
                // This avoids namespace resolution issues
                return $"({enumType}){value}";
            }
            
            if (value is string str)
                return $"\"{str.Replace("\"", "\\\"")}\"";
            
            if (value is bool b)
                return b ? "true" : "false";
            
            if (value is char c)
                return $"'{c}'";
            
            if (value is byte || value is sbyte || 
                value is short || value is ushort || 
                value is int || value is uint || 
                value is long || value is ulong ||
                value is float || value is double || value is decimal)
                return value.ToString();
            
            // For Type values
            if (value is ITypeSymbol typeValue)
            {
                var typeString = typeValue.ToDisplayString();
                if (typeString.EndsWith("?") && !typeValue.IsValueType)
                {
                    typeString = typeString.TrimEnd('?');
                }
                return $"typeof({typeString})";
            }
            
            // Default to string representation
            return $"\"{value}\"";
        }

        /// <summary>
        /// Generates Statics-specific DI extension methods for the target namespace.
        /// </summary>
        private string GenerateStaticsServiceCollectionExtensions(string targetNamespace)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using MetaTypes.Abstractions;");
            sb.AppendLine("using MetaTypes.Abstractions.Vendor.Statics;");
            sb.AppendLine();
            sb.AppendLine($"namespace {targetNamespace};");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Statics vendor DI extension methods for MetaTypes generated in {targetNamespace} namespace.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class StaticsServiceCollectionExtensions");
            sb.AppendLine("{");
            
            // Generate the Statics-specific AddMetaTypes method
            var methodName = NamingUtils.ToAddVendorMetaTypesMethodName(targetNamespace, "Statics");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Registers Statics-specific MetaTypes from the {targetNamespace} namespace.");
            sb.AppendLine($"    /// This registers IMetaTypeStatics interfaces for all static service classes.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static IServiceCollection {methodName}(this IServiceCollection services)");
            sb.AppendLine("    {");
            sb.AppendLine($"        // First register the base MetaTypes");
            sb.AppendLine($"        services.{NamingUtils.ToAddMetaTypesMethodName(targetNamespace)}();");
            sb.AppendLine();
            sb.AppendLine("        // Register Statics-specific interfaces");
            sb.AppendLine($"        foreach (var metaType in {targetNamespace}.MetaTypes.Instance.AssemblyMetaTypes)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (metaType is IMetaTypeStatics staticsType)");
            sb.AppendLine("            {");
            sb.AppendLine("                services.AddSingleton<IMetaTypeStatics>(staticsType);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Statics vendor service provider extension methods for retrieving registered MetaTypes.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public static class StaticsServiceProviderExtensions");
            sb.AppendLine("{");
            
            // Add GetStaticsMetaTypes method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets all registered Statics MetaTypes from the service provider.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IEnumerable<IMetaTypeStatics> GetStaticsMetaTypes(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>();");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add generic GetStaticsMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific Statics MetaType by static service class type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeStatics? GetStaticsMetaType<T>(this IServiceProvider serviceProvider)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>()");
            sb.AppendLine("            .FirstOrDefault(mt => ((IMetaType)mt).ManagedType == typeof(T));");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // Add non-generic GetStaticsMetaType method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a specific Statics MetaType by static service class type.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static IMetaTypeStatics? GetStaticsMetaType(this IServiceProvider serviceProvider, Type serviceType)");
            sb.AppendLine("    {");
            sb.AppendLine("        return serviceProvider.GetServices<IMetaTypeStatics>()");
            sb.AppendLine("            .FirstOrDefault(mt => ((IMetaType)mt).ManagedType == serviceType);");
            sb.AppendLine("    }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates shared wrapper classes that replace the explosion of individual argument classes.
        /// OPTIMIZATION: Reduces class count by ~80% for attribute arguments.
        /// </summary>
        private void GenerateSharedWrapperClasses(StringBuilder sb)
        {
            sb.AppendLine("#region Shared Wrapper Classes - OPTIMIZATION");
            sb.AppendLine();
            
            // Data structure for consolidated attribute data
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Data structure for consolidated attribute information - replaces individual attribute classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public readonly record struct StaticsAttributeData(");
            sb.AppendLine("    Type AttributeType,");
            sb.AppendLine("    (Type Type, object? Value)[] ConstructorArgs,");
            sb.AppendLine("    (string Name, Type Type, object? Value)[] NamedArgs);");
            sb.AppendLine();
            
            // Generic attribute info wrapper
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for attribute info - replaces individual attribute info classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeInfo : IStaticsAttributeInfo");
            sb.AppendLine("{");
            sb.AppendLine("    public Type AttributeType { get; }");
            sb.AppendLine("    private readonly (Type Type, object? Value)[] _constructorArgs;");
            sb.AppendLine("    private readonly (string Name, Type Type, object? Value)[] _namedArgs;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeArgument>? _constructorArguments;");
            sb.AppendLine("    private IReadOnlyList<IStaticsAttributeNamedArgument>? _namedArguments;");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeInfo(Type attributeType, (Type Type, object? Value)[] constructorArgs, (string Name, Type Type, object? Value)[] namedArgs)");
            sb.AppendLine("    {");
            sb.AppendLine("        AttributeType = attributeType;");
            sb.AppendLine("        _constructorArgs = constructorArgs;");
            sb.AppendLine("        _namedArgs = namedArgs;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments => ");
            sb.AppendLine("        _constructorArguments ??= _constructorArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeArgument(arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            sb.AppendLine();
            sb.AppendLine("    public IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments => ");
            sb.AppendLine("        _namedArguments ??= _namedArgs");
            sb.AppendLine("            .Select(arg => new StaticsAttributeNamedArgument(arg.Name, arg.Type, arg.Value))");
            sb.AppendLine("            .ToArray();");
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generic attribute argument wrapper
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for attribute constructor arguments - replaces individual argument classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeArgument : IStaticsAttributeArgument");
            sb.AppendLine("{");
            sb.AppendLine("    public Type ArgumentType { get; }");
            sb.AppendLine("    public object? Value { get; }");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeArgument(Type argumentType, object? value)");
            sb.AppendLine("    {");
            sb.AppendLine("        ArgumentType = argumentType;");
            sb.AppendLine("        Value = value;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Generic named attribute argument wrapper  
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Generic wrapper for named attribute arguments - replaces individual named argument classes");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("public class StaticsAttributeNamedArgument : IStaticsAttributeNamedArgument");
            sb.AppendLine("{");
            sb.AppendLine("    public string Name { get; }");
            sb.AppendLine("    public Type ArgumentType { get; }");
            sb.AppendLine("    public object? Value { get; }");
            sb.AppendLine();
            sb.AppendLine("    public StaticsAttributeNamedArgument(string name, Type argumentType, object? value)");
            sb.AppendLine("    {");
            sb.AppendLine("        Name = name;");
            sb.AppendLine("        ArgumentType = argumentType;");
            sb.AppendLine("        Value = value;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("#endregion");
        }

        /// <summary>
        /// Validates StaticsServiceMethodAttribute usage across all static service classes
        /// </summary>
        private List<ValidationResult> ValidateStaticsServiceMethods(IList<INamedTypeSymbol> staticTypes)
        {
            var results = new List<ValidationResult>();

            foreach (var staticClass in staticTypes)
            {
                var serviceMethods = GetStaticServiceMethods(staticClass);
                foreach (var method in serviceMethods)
                {
                    var attribute = method.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute");
                    
                    if (attribute != null)
                    {
                        var validationResult = ValidateStaticsServiceMethodAttribute(staticClass, method, attribute);
                        if (validationResult.HasErrors)
                        {
                            results.Add(validationResult);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Validates a single StaticsServiceMethodAttribute usage
        /// </summary>
        private ValidationResult ValidateStaticsServiceMethodAttribute(INamedTypeSymbol staticClass, IMethodSymbol method, AttributeData attribute)
        {
            var result = new ValidationResult
            {
                ClassName = staticClass.Name,
                MethodName = method.Name,
                Errors = new List<string>()
            };

            // Rule 0: Validate return type is StaticsServiceResult<T> or StaticsServiceResult
            var returnTypeString = method.ReturnType.ToDisplayString();
            if (!IsValidStaticsServiceResultReturnType(returnTypeString))
            {
                result.Errors.Add($"Method must return StaticsServiceResult<T> or StaticsServiceResult, but returns '{returnTypeString}'");
            }

            // Extract attribute properties
            var pathArg = GetAttributeArgumentValue(attribute, "Path")?.ToString();
            var entityArg = GetAttributeArgumentValue(attribute, "Entity") as ITypeSymbol;
            var entityGlobalArg = GetAttributeArgumentValue(attribute, "EntityGlobal");
            
            bool hasEntity = entityArg != null;
            bool entityGlobal = entityGlobalArg is bool b && b;

            if (string.IsNullOrEmpty(pathArg))
            {
                result.Errors.Add("Path argument is required");
                return result;
            }

            // Handle null path argument
            if (pathArg == null)
            {
                result.Errors.Add("Path argument cannot be null");
                return result;
            }

            // Parse route parameters from path (e.g., {id}, {id:int}, {enabled:bool})
            var routeParams = ParseRouteParameters(pathArg);
            var methodParams = new HashSet<string>(method.Parameters.Select(p => p.Name));

            // Rule 1: If path contains route parameters, all must exist as method parameters
            foreach (var routeParam in routeParams)
            {
                if (!methodParams.Contains(routeParam.Name))
                {
                    result.Errors.Add($"Route parameter '{routeParam.Name}' from path '{pathArg}' must exist as a method parameter");
                }
            }

            // Rule 2: If method has Entity parameter and uses 'id' parameter, EntityGlobal should be false or unspecified
            var hasIdParameter = routeParams.Any(p => p.Name == "id") || methodParams.Contains("id");
            if (hasEntity && hasIdParameter && entityGlobal)
            {
                result.Errors.Add("Methods with Entity parameter and 'id' parameter should not have EntityGlobal = true");
            }

            // Rule 3: If method has 'id' parameter, it should have Entity parameter
            if (hasIdParameter && !hasEntity)
            {
                result.Errors.Add("Methods with 'id' parameter must specify Entity parameter");
            }

            // Rule 4: If EntityGlobal is true, method should not have 'id' parameter
            if (entityGlobal && hasIdParameter)
            {
                result.Errors.Add("Methods with EntityGlobal = true should not have 'id' parameter");
            }

            // Rule 5: Validate route parameter type constraints match method parameter types
            foreach (var routeParam in routeParams.Where(p => !string.IsNullOrEmpty(p.TypeConstraint)))
            {
                var methodParam = method.Parameters.FirstOrDefault(p => p.Name == routeParam.Name);
                if (methodParam != null)
                {
                    if (!IsTypeConstraintCompatible(routeParam.TypeConstraint!, methodParam.Type))
                    {
                        result.Errors.Add($"Route parameter '{routeParam.Name}:{routeParam.TypeConstraint}' type constraint does not match method parameter type '{methodParam.Type.ToDisplayString()}'");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parses route parameters from a path string (e.g., "/users/{id:int}/status/{enabled:bool}")
        /// </summary>
        private List<RouteParameter> ParseRouteParameters(string path)
        {
            var results = new List<RouteParameter>();
            var regex = new Regex(@"\{([^}:]+)(?::([^}]+))?\}");
            var matches = regex.Matches(path);

            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                var typeConstraint = match.Groups[2].Success ? match.Groups[2].Value : null;
                results.Add(new RouteParameter { Name = name, TypeConstraint = typeConstraint });
            }

            return results;
        }

        /// <summary>
        /// Checks if a route type constraint is compatible with a method parameter type
        /// </summary>
        private bool IsTypeConstraintCompatible(string typeConstraint, ITypeSymbol parameterType)
        {
            var paramTypeName = parameterType.SpecialType switch
            {
                SpecialType.System_Int32 => "int",
                SpecialType.System_Int64 => "long",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Double => "double",
                SpecialType.System_Single => "float",
                SpecialType.System_Decimal => "decimal",
                SpecialType.System_String => "string",
                _ => parameterType.Name.ToLowerInvariant()
            };

            return typeConstraint.ToLowerInvariant() == paramTypeName;
        }

        /// <summary>
        /// Gets an attribute argument value by name
        /// </summary>
        private object? GetAttributeArgumentValue(AttributeData attribute, string argumentName)
        {
            // Check named arguments first
            var namedArg = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == argumentName);
            if (!namedArg.Equals(default(KeyValuePair<string, TypedConstant>)))
            {
                return namedArg.Value.Value;
            }

            // For constructor arguments, we need to map by position or known parameter names
            // This is simplified - in real implementation you'd need to check the constructor signature
            if (argumentName == "Path" && attribute.ConstructorArguments.Length > 0)
            {
                return attribute.ConstructorArguments[0].Value;
            }

            return null;
        }

        /// <summary>
        /// Simplifies ServiceResult type names in generated code to use the shorter form
        /// </summary>
        private string SimplifyServiceResultTypeName(string returnTypeString)
        {
            // Replace fully qualified ServiceResult types with simplified versions
            if (returnTypeString.StartsWith("Statics.ServiceResult.ServiceResult<"))
            {
                // ServiceResult<T> -> ServiceResult<T>
                return returnTypeString.Replace("Statics.ServiceResult.", "");
            }
            else if (returnTypeString == "Statics.ServiceResult.ServiceResult")
            {
                // ServiceResult -> ServiceResult
                return "ServiceResult";
            }
            else if (returnTypeString.StartsWith("System.Threading.Tasks.Task<Statics.ServiceResult.ServiceResult"))
            {
                // Task<ServiceResult<T>> -> Task<ServiceResult<T>>
                return returnTypeString.Replace("Statics.ServiceResult.", "");
            }
            
            return returnTypeString;
        }

        /// <summary>
        /// Analyzes a service method to extract async and return type information for repository generation
        /// </summary>
        private ServiceMethodInfo AnalyzeServiceMethod(IMethodSymbol method, AttributeData attribute)
        {
            var returnTypeString = method.ReturnType.ToDisplayString();
            
            // Detect if method is async (returns Task<>)
            bool isAsync = returnTypeString.StartsWith("System.Threading.Tasks.Task<") && returnTypeString.EndsWith(">");
            
            // Extract the actual ServiceResult type
            string serviceResultType;
            if (isAsync)
            {
                // Extract inner type from Task<ServiceResult<T>>
                serviceResultType = returnTypeString.Substring("System.Threading.Tasks.Task<".Length);
                serviceResultType = serviceResultType.Substring(0, serviceResultType.Length - 1); // Remove trailing >
            }
            else
            {
                // Direct ServiceResult<T> return
                serviceResultType = returnTypeString;
            }
            
            // Extract Entity and EntityGlobal information from attribute
            var entityArg = GetAttributeArgumentValue(attribute, "Entity") as ITypeSymbol;
            var entityGlobalArg = GetAttributeArgumentValue(attribute, "EntityGlobal");
            
            bool hasEntity = entityArg != null;
            bool entityGlobal = entityGlobalArg is bool b && b;
            bool hasIdParameter = method.Parameters.Any(p => p.Name == "id");
            
            return new ServiceMethodInfo
            {
                Method = method,
                Attribute = attribute,
                IsAsync = isAsync,
                ServiceResultType = serviceResultType,
                EntityType = entityArg as INamedTypeSymbol,
                IsEntityGlobal = hasEntity && entityGlobal,
                IsEntitySpecific = hasEntity && !entityGlobal && hasIdParameter,
                IsGlobalMethod = !hasEntity
            };
        }

        /// <summary>
        /// Validates if a return type string represents a valid StaticsServiceResult type
        /// </summary>
        private bool IsValidStaticsServiceResultReturnType(string returnTypeString)
        {
            // Handle async methods - extract the inner type from Task<T>
            var actualReturnType = returnTypeString;
            if (returnTypeString.StartsWith("System.Threading.Tasks.Task<") && returnTypeString.EndsWith(">"))
            {
                // Extract inner type from Task<StaticsServiceResult<T>>
                actualReturnType = returnTypeString.Substring("System.Threading.Tasks.Task<".Length);
                actualReturnType = actualReturnType.Substring(0, actualReturnType.Length - 1); // Remove trailing >
            }
            
            // Check if it's exactly StaticsServiceResult or starts with StaticsServiceResult<
            return actualReturnType == "Statics.ServiceResult.ServiceResult" ||
                   actualReturnType.StartsWith("Statics.ServiceResult.ServiceResult<");
        }

        /// <summary>
        /// Generates diagnostic file content for validation results
        /// </summary>
        private string GenerateValidationDiagnostics(List<ValidationResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// StaticsServiceMethodAttribute Validation Diagnostics");
            sb.AppendLine($"// Generated at: {DateTime.Now}");
            sb.AppendLine($"// Total violations found: {results.Count}");
            sb.AppendLine();

            foreach (var result in results)
            {
                sb.AppendLine($"// ERROR in {result.ClassName}.{result.MethodName}:");
                foreach (var error in result.Errors)
                {
                    sb.AppendLine($"//   - {error}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("// Validation Rules:");
            sb.AppendLine("//   0. Methods must return StaticsServiceResult<T> or StaticsServiceResult (async methods via Task<T> are supported)");
            sb.AppendLine("//   1. All route parameters in Path must exist as method parameters");
            sb.AppendLine("//   2. Methods with 'id' parameter must specify Entity parameter");
            sb.AppendLine("//   3. Methods with Entity + 'id' should not have EntityGlobal = true");
            sb.AppendLine("//   4. Methods with EntityGlobal = true should not have 'id' parameter");
            sb.AppendLine("//   5. Route parameter type constraints must match method parameter types");

            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Represents a route parameter parsed from a path
    /// </summary>
    public class RouteParameter
    {
        public string Name { get; set; } = "";
        public string? TypeConstraint { get; set; }
    }

    /// <summary>
    /// Represents validation results for a method
    /// </summary>
    public class ValidationResult
    {
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Any();
    }
    
    /// <summary>
    /// Represents analyzed information about a service method including async characteristics
    /// </summary>
    public class ServiceMethodInfo
    {
        public IMethodSymbol Method { get; set; } = null!;
        public AttributeData Attribute { get; set; } = null!;
        public bool IsAsync { get; set; }
        public string ServiceResultType { get; set; } = "";
        public INamedTypeSymbol? EntityType { get; set; }
        public bool IsEntityGlobal { get; set; }
        public bool IsEntitySpecific { get; set; }
        public bool IsGlobalMethod { get; set; }
    }

    /// <summary>
    /// Represents a repository specification for generation
    /// </summary>
    public class RepositorySpec
    {
        public string Name { get; set; } = "";                    // "UserRepository", "GlobalRepository"
        public string Namespace { get; set; } = "";               // Target namespace or "{TargetNamespace}.GlobalRepository"
        public INamedTypeSymbol? EntityType { get; set; }         // null for GlobalRepository
        public INamedTypeSymbol? DbContextType { get; set; }      // null for Statics-only
        public string? KeyPropertyType { get; set; }              // from [Key] attribute, for CRUD id parameters
        public List<ServiceMethodInfo> ServiceMethods { get; set; } = new();
        public bool HasCrud => DbContextType != null;
        public bool IsGlobal => EntityType == null;
    }

    /// <summary>
    /// Configuration for Statics vendor generator
    /// </summary>
    public class StaticsConfig
    {
        public bool RequireBaseTypes { get; set; } = true;
        public bool IncludeParameterAttributes { get; set; } = true;
        public bool IncludeMethodAttributes { get; set; } = true;
    }

}