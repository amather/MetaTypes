namespace MetaTypes.Abstractions.Vendor.Statics;

/// <summary>
/// Extends IMetaType with Statics vendor specific metadata for static service methods.
/// </summary>
public interface IMetaTypeStatics
{
    /// <summary>
    /// Gets all static service methods discovered in this type that have the StaticsServiceMethodAttribute.
    /// </summary>
    IReadOnlyList<IStaticsServiceMethod> ServiceMethods { get; }
}