using System;
using System.Linq;
using System.Text;

namespace MetaTypes.Generator.Common.Generator;

/// <summary>
/// Shared utilities for consistent naming across all generators.
/// </summary>
public static class NamingUtils
{
    /// <summary>
    /// Converts a simple string to PascalCase by capitalizing the first character.
    /// Originally from StaticsServiceMethodVendorGenerator.cs for parameter name conversion.
    /// </summary>
    /// <param name="input">The input string to convert</param>
    /// <returns>PascalCase string</returns>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }
    
    /// <summary>
    /// Converts a namespace or complex identifier to a valid PascalCase method name.
    /// Handles dots, hyphens, underscores, and other special characters.
    /// </summary>
    /// <param name="input">The input namespace or identifier (e.g., "Sample.EfCore.SingleProject")</param>
    /// <returns>PascalCase method name (e.g., "SampleEfCoreSingleProject")</returns>
    public static string ToMethodName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Split on common separators: dots, hyphens, underscores, spaces
        var segments = input.Split(['.', '-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        
        var result = new StringBuilder();
        foreach (var segment in segments)
        {
            if (!string.IsNullOrEmpty(segment))
            {
                // Convert each segment to PascalCase and append
                result.Append(ToPascalCase(segment));
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Generates a DI extension method name from a target namespace.
    /// Format: "AddMetaTypes{PascalCaseNamespace}"
    /// </summary>
    /// <param name="targetNamespace">The target namespace where generator runs</param>
    /// <returns>DI method name (e.g., "AddMetaTypesSampleConsole")</returns>
    public static string ToAddMetaTypesMethodName(string targetNamespace)
    {
        var pascalCaseNamespace = ToMethodName(targetNamespace);
        return $"AddMetaTypes{pascalCaseNamespace}";
    }
    
    /// <summary>
    /// Generates a vendor-specific DI extension method name from a target namespace and vendor name.
    /// Format: "AddMetaTypes{PascalCaseNamespace}{VendorName}"
    /// </summary>
    /// <param name="targetNamespace">The target namespace where generator runs</param>
    /// <param name="vendorName">The vendor name (e.g., "EfCore", "Statics")</param>
    /// <returns>Vendor DI method name (e.g., "AddMetaTypesSampleConsoleEfCore")</returns>
    public static string ToAddVendorMetaTypesMethodName(string targetNamespace, string vendorName)
    {
        var pascalCaseNamespace = ToMethodName(targetNamespace);
        return $"AddMetaTypes{pascalCaseNamespace}{vendorName}";
    }
}