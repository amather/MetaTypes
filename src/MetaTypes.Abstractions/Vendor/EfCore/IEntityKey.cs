using System;
using System.Linq.Expressions;

namespace MetaTypes.Abstractions.Vendor.EfCore;

/// <summary>
/// Represents a strongly-typed key for an entity.
/// </summary>
/// <typeparam name="TEntity">The entity type this key identifies</typeparam>
public interface IEntityKey<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an expression that can be used to query for the entity by its key.
    /// </summary>
    Expression<Func<TEntity, bool>> Where { get; }
}
