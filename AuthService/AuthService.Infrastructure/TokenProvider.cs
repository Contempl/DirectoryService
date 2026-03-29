using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Core.Options;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Kernel;

namespace AuthService.Core;

public class TokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<TokenProvider> _logger;

    public TokenProvider(IOptions<JwtOptions> jwtOptions, ILogger<TokenProvider> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, string.Concat(user.FirstName, " ", user.LastName)),
            new Claim("jti", Guid.NewGuid().ToString()),
        };
        
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
    
    public Result<RefreshToken, Error> GenerateRefreshToken(Guid userId, Guid jwtTokenId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var expiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetime);
        
        var refreshTokenResult =  RefreshToken.Create(token, userId, jwtTokenId, expiryDate);
        if (refreshTokenResult.IsFailure)
        {
            _logger.LogInformation("failed to create refresh token.");
            return refreshTokenResult.Error;
        }
        
        var refreshToken = refreshTokenResult.Value;

        return refreshToken;
    }
}


// sub — Id пользователя (Guid)
// email — email пользователя
// name — FirstName + LastName
// jti — уникальный идентификатор токена
// roles — список ролей пользователя (каждая роль — отдельный claim)
// permissions — список permissions на основе ролей (каждый permission — отдельный claim)