namespace MetaTypes.Abstractions;

/// <summary>
/// Extends IMetaType with Entity Framework Core specific metadata.
/// </summary>
public interface IMetaTypeEfCore
{
    /// <summary>
    /// Gets the table name for this entity type, if configured via [Table] attribute or fluent API.
    /// </summary>
    string? TableName { get; }
    
    /// <summary>
    /// Gets the primary key members for this entity type.
    /// </summary>
    IReadOnlyList<IMetaTypeMemberEfCore> Keys { get; }
}