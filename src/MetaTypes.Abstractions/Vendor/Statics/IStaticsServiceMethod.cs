using System;
using MetaTypes.Abstractions;

namespace MetaTypes.Abstractions.Vendor.Statics;

/// <summary>
/// Represents a static service method with complete metadata including attributes.
/// </summary>
public interface IStaticsServiceMethod
{
    /// <summary>
    /// Gets the name of the static method.
    /// </summary>
    string MethodName { get; }
    
    /// <summary>
    /// Gets the return type of the static method.
    /// </summary>
    Type ReturnType { get; }
    
    /// <summary>
    /// Gets all attributes applied to the static method with their arguments/values.
    /// </summary>
    IReadOnlyList<IStaticsAttributeInfo> MethodAttributes { get; }
    
    /// <summary>
    /// Gets all parameters of the static method including their attributes.
    /// </summary>
    IReadOnlyList<IStaticsParameterInfo> Parameters { get; }
}

/// <summary>
/// Represents a parameter of a static service method.
/// </summary>
public interface IStaticsParameterInfo
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    string ParameterName { get; }
    
    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type ParameterType { get; }
    
    /// <summary>
    /// Gets all attributes applied to this parameter with their arguments/values.
    /// </summary>
    IReadOnlyList<IStaticsAttributeInfo> ParameterAttributes { get; }
}

/// <summary>
/// Represents attribute information with arguments and values.
/// </summary>
public interface IStaticsAttributeInfo
{
    /// <summary>
    /// Gets the attribute type.
    /// </summary>
    Type AttributeType { get; }
    
    /// <summary>
    /// Gets the constructor arguments passed to the attribute.
    /// </summary>
    IReadOnlyList<IStaticsAttributeArgument> ConstructorArguments { get; }
    
    /// <summary>
    /// Gets the named arguments (properties/fields) set on the attribute.
    /// </summary>
    IReadOnlyList<IStaticsAttributeNamedArgument> NamedArguments { get; }
}

/// <summary>
/// Represents a constructor argument of an attribute.
/// </summary>
public interface IStaticsAttributeArgument
{
    /// <summary>
    /// Gets the type of the argument.
    /// </summary>
    Type ArgumentType { get; }
    
    // Note: Each concrete implementation should provide its own strongly-typed Value property
    // matching the ArgumentType (e.g., public bool Value { get; } for bool arguments)
}

/// <summary>
/// Represents a named argument (property or field) of an attribute.
/// </summary>
public interface IStaticsAttributeNamedArgument
{
    /// <summary>
    /// Gets the name of the property or field.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the type of the argument.
    /// </summary>
    Type ArgumentType { get; }
    
    // Note: Each concrete implementation should provide its own strongly-typed Value property
    // matching the ArgumentType (e.g., public bool? Value { get; } for bool? arguments)
}