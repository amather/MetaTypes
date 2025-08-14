using System;
using System.Collections.Generic;

namespace MetaTypes.Abstractions.Vendor.EfCore;

/// <summary>
/// Represents a DbContext and its associated entity types with EF Core metadata.
/// Enables consumers to iterate through DbContexts and access their entity types.
/// </summary>
public interface IMetaTypesEfCoreDbContext
{
    /// <summary>
    /// Gets the name of the DbContext class.
    /// </summary>
    string ContextName { get; }
    
    /// <summary>
    /// Gets the Type of the DbContext class.
    /// </summary>
    Type ContextType { get; }
    
    /// <summary>
    /// Gets all entity types (with EF Core metadata) that belong to this DbContext.
    /// </summary>
    IEnumerable<IMetaTypeEfCore> EntityTypes { get; }
    
    // Future: void ConfigureModel(ModelBuilder modelBuilder);
}