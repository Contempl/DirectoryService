namespace AuthService.Application.Features.RefreshToken;

public record RefreshResult(
    string AccessToken,
    int ExpiresIn,
    string RawRefreshToken,
    DateTime RefreshTokenExpiresAt);