using AuthService.Application.Abstractions;

namespace AuthService.Core.Options;

public record JwtOptions : IJwtOptions
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; }

    public int RefreshTokenLifetime { get; init; }
}