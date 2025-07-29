namespace MetaTypes.Abstractions;

public interface IMetaTypeMember
{
    string MemberName { get; }
    Type MemberType { get; }
    bool IsMetaType { get; }
    IMetaType? MetaType { get; }
    bool HasSetter { get; }
    bool IsList { get; }
    Type[]? GenericTypeArguments { get; }
    Attribute[]? Attributes { get; }
    
    /// <summary>
    /// Gets the value of this property from the specified object instance.
    /// </summary>
    /// <param name="obj">The object instance to get the property value from.</param>
    /// <returns>The value of the property.</returns>
    object? GetValue(object obj);
    
    /// <summary>
    /// Sets the value of this property on the specified object instance.
    /// </summary>
    /// <param name="obj">The object instance to set the property value on.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(object obj, object? value);
}