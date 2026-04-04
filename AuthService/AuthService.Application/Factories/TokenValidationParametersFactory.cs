using System.Text;
using AuthService.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Factories;

public static class TokenValidationParametersFactory
{
    public static TokenValidationParameters Create(IJwtOptions jwtOptions, bool validateLifetime = true)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = validateLifetime
        };
    }
}