namespace AuthService.Configuration;

public sealed class AuthorizationServerOptions
{
    public const string SectionName = "AuthorizationServer";

    public string Issuer { get; init; } = string.Empty;
    public string LoginPageUrl { get; init; } = string.Empty;
    public string ApiScope { get; init; } = "directory_api";
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 7;
    public string? SigningCertificatePath { get; init; }
    public string? SigningCertificatePassword { get; init; }
    public string? EncryptionCertificatePath { get; init; }
    public string? EncryptionCertificatePassword { get; init; }
}
