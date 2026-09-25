using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application;
using AuthService.Core.Options;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Kernel;

namespace AuthService.Core;

public class TokenProvider : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<TokenProvider> _logger;

    public TokenProvider(IOptions<JwtOptions> jwtOptions, ILogger<TokenProvider> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public string GenerateJwtToken(ApplicationUser user, List<string> roles, HashSet<string> permissions)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Name, string.Concat(user.FirstName, " ", user.LastName)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.UtcNow).ToString(),
                ClaimValueTypes.Integer64),
        };
        
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));
        
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var jwtSecurityToken = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            signingCredentials: credentials);
        
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        return token;
    }

    public Result<GeneratedRefreshToken, Error> GenerateInitialRefreshToken(Guid userId, string jwtId)
    {
        var tokenPair = GenerateTokenPair();
        
        var expiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetime);
        
        var refreshTokenResult = RefreshToken.CreateInitial(tokenPair.TokenHash, userId, jwtId, expiryDate);
        
        if (refreshTokenResult.IsFailure)
        {
            _logger.LogInformation(
                "Failed to create initial refresh token.");

            return refreshTokenResult.Error;
        }
        
        return new GeneratedRefreshToken(tokenPair.RawToken, refreshTokenResult.Value);
    }

    public Result<GeneratedRefreshToken, Error> GenerateRotatedRefreshToken(Guid userId, string jwtId, Guid familyId)
    {
        var tokenPair = GenerateTokenPair();
        
        var expiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetime);
        
        var refreshTokenResult = RefreshToken.CreateRotated(tokenPair.TokenHash, userId, jwtId, expiryDate, familyId);
        
        if (refreshTokenResult.IsFailure)
        {
            _logger.LogInformation(
                "Failed to create rotated refresh token.");

            return refreshTokenResult.Error;
        }
        
        return new GeneratedRefreshToken(tokenPair.RawToken, refreshTokenResult.Value);
    }

    public string HashRefreshToken(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)))
            .ToLowerInvariant();
    }

    private (string RawToken, string TokenHash) GenerateTokenPair()
    {
        var rawToken = Base64UrlEncoder.Encode(
            RandomNumberGenerator.GetBytes(64));

        var tokenHash = HashRefreshToken(rawToken);

        return (rawToken, tokenHash);
    }
}
