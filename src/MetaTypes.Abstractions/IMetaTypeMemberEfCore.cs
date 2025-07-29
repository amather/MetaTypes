namespace MetaTypes.Abstractions;

/// <summary>
/// Extends IMetaTypeMember with Entity Framework Core specific metadata.
/// </summary>
public interface IMetaTypeMemberEfCore
{
    /// <summary>
    /// Gets a value indicating whether this property is part of the primary key.
    /// </summary>
    bool IsKey { get; }
    
    /// <summary>
    /// Gets a value indicating whether this property represents a foreign key relationship.
    /// </summary>
    bool IsForeignKey { get; }
    
    /// <summary>
    /// Gets a value indicating whether this property is marked with [NotMapped] and should not be persisted to the database.
    /// </summary>
    bool IsNotMapped { get; }
    
    /// <summary>
    /// Gets the referenced member for foreign key relationships, if applicable.
    /// </summary>
    IMetaTypeMember? ForeignKeyMember { get; }
}