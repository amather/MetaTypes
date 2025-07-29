using MetaTypes.Abstractions;

namespace Sample.Business.Dto;

[MetaType]
public record CustomerResponse(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    int AddressCount
);