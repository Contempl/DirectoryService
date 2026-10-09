using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Auth;
using DirectoryService.Infrastructure.Options;
using Framework.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DirectoryService.Infrastructure.Auth;

internal sealed class ServiceTokenProvider(
    IOptions<JwtOptions> jwtOptions,
    IOptions<ServiceTokenOptions> serviceTokenOptions,
    TimeProvider timeProvider)
    : IServiceTokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly ServiceTokenOptions _serviceTokenOptions =
        serviceTokenOptions.Value;

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _expiresAt;

    public async ValueTask<string> GetTokenAsync(
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (IsTokenUsable(now))
        {
            return _cachedToken!;
        }

        await _refreshLock.WaitAsync(cancellationToken);

        try
        {
            now = timeProvider.GetUtcNow();

            if (IsTokenUsable(now))
            {
                return _cachedToken!;
            }

            _expiresAt = now.AddMinutes(
                _serviceTokenOptions.LifetimeMinutes);
            _cachedToken = GenerateToken(now, _expiresAt);

            return _cachedToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool IsTokenUsable(DateTimeOffset now)
    {
        if (_cachedToken is null)
        {
            return false;
        }

        var refreshAt = _expiresAt.AddSeconds(
            -_serviceTokenOptions.RefreshBeforeExpirationSeconds);

        return now < refreshAt;
    }

    private string GenerateToken(
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                _serviceTokenOptions.SubjectId.ToString()),
            new(
                JwtRegisteredClaimNames.Name,
                _serviceTokenOptions.ServiceName),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(issuedAt.UtcDateTime).ToString(),
                ClaimValueTypes.Integer64),
            new(JwtClaimTypes.Role, SystemRoles.ServiceAccount)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
