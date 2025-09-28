using MetaTypes.Generator.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaTypes.Generator.Discovery;

/// <summary>
/// Result of executing discovery methods.
/// </summary>
public class DiscoveryExecutionResult
{
    public List<DiscoveredType> DiscoveredTypes { get; set; } = [];
    public List<string> Debug { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public bool Success { get; set; }
    public List<string> MethodsUsed { get; set; } = [];
}
