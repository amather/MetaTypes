using MetaTypes.Abstractions;

namespace MetaTypes.Abstractions.Vendor.Statics;

/// <summary>
/// Extends IMetaTypeMember with Statics vendor specific metadata for static service members.
/// </summary>
public interface IMetaTypeMemberStatics
{
    /// <summary>
    /// Gets a value indicating whether this member is a static service property or field.
    /// </summary>
    bool IsStaticService { get; }
    
    /// <summary>
    /// Gets the static service attributes applied to this member, if applicable.
    /// </summary>
    IReadOnlyList<IStaticsAttributeInfo> StaticServiceAttributes { get; }
}