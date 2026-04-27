using System.Text;
using Framework.Options;
using Microsoft.IdentityModel.Tokens;

namespace Framework.Factories;

public static class TokenValidationParametersFactory
{
    public static TokenValidationParameters Create(IJwtOptions jwtOptions, bool validateLifetime = true)
    {
        Console.WriteLine($"Factory Secret: {jwtOptions.Secret}");
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