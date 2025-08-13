using System;

namespace Sample.Statics.ServiceMethod.Attributes;

/// <summary>
/// Custom attribute to demonstrate constructor arguments vs named arguments
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomServiceAttribute : Attribute
{
    // Constructor arguments - these are positional parameters
    public string ServiceName { get; }
    public int Priority { get; }
    
    // Named arguments - these are properties that can be set optionally
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Type? ReturnType { get; set; }

    // Constructor with required parameters
    public CustomServiceAttribute(string serviceName, int priority)
    {
        ServiceName = serviceName;
        Priority = priority;
    }
}