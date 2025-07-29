using System.Linq.Expressions;

namespace MetaTypes.Abstractions;

public interface IMetaType
{
    Type ManagedType { get; }
    string ManagedTypeName { get; }
    string ManagedTypeNamespace { get; }
    string ManagedTypeAssembly { get; }
    string ManagedTypeFullName { get; }
    Type[]? GenericTypeArguments { get; }
    Attribute[]? Attributes { get; }
    IReadOnlyList<IMetaTypeMember> Members { get; }

    IMetaTypeMember? FindMember(string name);
    IMetaTypeMember FindRequiredMember(string name);
}

public interface IMetaType<T> : IMetaType
{
    IMetaTypeMember? FindMember<TProperty>(Expression<Func<T, TProperty>> expression);
    IMetaTypeMember FindRequiredMember<TProperty>(Expression<Func<T, TProperty>> expression);
}