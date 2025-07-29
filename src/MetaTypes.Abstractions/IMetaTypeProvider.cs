namespace MetaTypes.Abstractions;

public interface IMetaTypeProvider
{
    IReadOnlyList<IMetaType> AssemblyMetaTypes { get; }
}