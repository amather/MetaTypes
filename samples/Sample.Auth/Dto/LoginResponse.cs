using MetaTypes.Abstractions;

namespace Sample.Auth.Dto;

[MetaType]
public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken
);