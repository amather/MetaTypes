using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MetaTypes.Generator.Diagnostics;

namespace MetaTypes.Generator.Vendor.Statics.Diagnostics;

/// <summary>
/// Diagnostic analyzer provider for Statics.ServiceMethod discovery method.
/// Validates [StaticsServiceMethod] attribute usage on static methods.
/// </summary>
public class StaticsServiceMethodDiagnosticProvider : IDiagnosticAnalyzerProvider
{
    public string Identifier => "Statics.ServiceMethod";

    public string Description => "Validates StaticsServiceMethod attribute usage on static methods";

    public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => new[]
    {
        StaticsDiagnostics.MTSTAT0001_InvalidReturnType,
        StaticsDiagnostics.MTSTAT0002_MissingRouteParameter,
        StaticsDiagnostics.MTSTAT0003_MissingEntityParameter,
        StaticsDiagnostics.MTSTAT0004_EntityGlobalWithIdParameter,
        StaticsDiagnostics.MTSTAT0005_EntityWithIdShouldNotBeGlobal,
        StaticsDiagnostics.MTSTAT0006_RouteParameterTypeMismatch,
        StaticsDiagnostics.MTSTAT0007_MissingPathParameter,
    };

    public void Analyze(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        ImmutableArray<AttributeData> relevantAttributes)
    {
        // Only analyze static classes
        if (!typeSymbol.IsStatic)
            return;

        // Find all static methods with StaticsServiceMethodAttribute
        var serviceMethods = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsStatic && (m.DeclaredAccessibility == Accessibility.Public || m.DeclaredAccessibility == Accessibility.Internal))
            .Where(m => HasStaticsServiceMethodAttribute(m));

        foreach (var method in serviceMethods)
        {
            var attribute = method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute");

            if (attribute != null)
            {
                ValidateServiceMethod(context, method, attribute);
            }
        }
    }

    private bool HasStaticsServiceMethodAttribute(IMethodSymbol method)
    {
        return method.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "Statics.ServiceBroker.Attributes.StaticsServiceMethodAttribute");
    }

    private void ValidateServiceMethod(SymbolAnalysisContext context, IMethodSymbol method, AttributeData attribute)
    {
        // Rule 0: Validate return type is ServiceResult<T> or ServiceResult
        var returnTypeString = method.ReturnType.ToDisplayString();
        if (!IsValidServiceResultReturnType(returnTypeString))
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0001_InvalidReturnType,
                method.Locations[0],
                method.Name,
                returnTypeString);
            context.ReportDiagnostic(diagnostic);
        }

        // Extract attribute properties
        var pathArg = GetAttributeArgumentValue(attribute, "Path")?.ToString();
        var entityArg = GetAttributeArgumentValue(attribute, "Entity") as ITypeSymbol;
        var entityGlobalArg = GetAttributeArgumentValue(attribute, "EntityGlobal");

        bool hasEntity = entityArg != null;
        bool entityGlobal = entityGlobalArg is bool b && b;

        // Rule: Path is required
        if (string.IsNullOrEmpty(pathArg))
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0007_MissingPathParameter,
                method.Locations[0],
                method.Name);
            context.ReportDiagnostic(diagnostic);
            return; // Can't validate further without path
        }

        // Parse route parameters from path (e.g., {id}, {id:int}, {enabled:bool})
        var routeParams = ParseRouteParameters(pathArg!);
        var methodParams = new HashSet<string>(method.Parameters.Select(p => p.Name));

        // Rule 1: If path contains route parameters, all must exist as method parameters
        foreach (var routeParam in routeParams)
        {
            if (!methodParams.Contains(routeParam.Name))
            {
                var diagnostic = Diagnostic.Create(
                    StaticsDiagnostics.MTSTAT0002_MissingRouteParameter,
                    method.Locations[0],
                    routeParam.Name,
                    pathArg);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // Rule 2: If method has 'id' parameter, it should have Entity parameter
        var hasIdParameter = routeParams.Any(p => p.Name == "id") || methodParams.Contains("id");
        if (hasIdParameter && !hasEntity)
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0003_MissingEntityParameter,
                method.Locations[0],
                method.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Rule 3: If method has Entity parameter and uses 'id' parameter, EntityGlobal should be false or unspecified
        if (hasEntity && hasIdParameter && entityGlobal)
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0005_EntityWithIdShouldNotBeGlobal,
                method.Locations[0],
                method.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Rule 4: If EntityGlobal is true, method should not have 'id' parameter
        if (entityGlobal && hasIdParameter)
        {
            var diagnostic = Diagnostic.Create(
                StaticsDiagnostics.MTSTAT0004_EntityGlobalWithIdParameter,
                method.Locations[0],
                method.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Rule 5: Validate route parameter type constraints match method parameter types
        foreach (var routeParam in routeParams.Where(p => !string.IsNullOrEmpty(p.TypeConstraint)))
        {
            var methodParam = method.Parameters.FirstOrDefault(p => p.Name == routeParam.Name);
            if (methodParam != null)
            {
                if (!IsTypeConstraintCompatible(routeParam.TypeConstraint!, methodParam.Type))
                {
                    var diagnostic = Diagnostic.Create(
                        StaticsDiagnostics.MTSTAT0006_RouteParameterTypeMismatch,
                        method.Locations[0],
                        routeParam.Name,
                        routeParam.TypeConstraint,
                        methodParam.Type.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private bool IsValidServiceResultReturnType(string returnTypeString)
    {
        // Handle async methods - extract the inner type from Task<T>
        var actualReturnType = returnTypeString;
        if (returnTypeString.StartsWith("System.Threading.Tasks.Task<") && returnTypeString.EndsWith(">"))
        {
            // Extract inner type from Task<ServiceResult<T>>
            actualReturnType = returnTypeString.Substring("System.Threading.Tasks.Task<".Length);
            actualReturnType = actualReturnType.Substring(0, actualReturnType.Length - 1); // Remove trailing >
        }

        // Check if it's exactly ServiceResult or starts with ServiceResult<
        return actualReturnType == "Statics.ServiceResult.ServiceResult" ||
               actualReturnType.StartsWith("Statics.ServiceResult.ServiceResult<");
    }

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

    private class RouteParameter
    {
        public string Name { get; set; } = "";
        public string? TypeConstraint { get; set; }
    }
}
