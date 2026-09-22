using Core.Auth;
using System.Text;
using Framework.Authorization;
using Framework.Factories;
using Framework.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Middleware;

public static class Extensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptionsSection = configuration.GetRequiredSection(nameof(JwtOptions));
        var jwtOptions = jwtOptionsSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{nameof(JwtOptions)}' is invalid.");

        services.AddOptions<JwtOptions>()
            .Bind(jwtOptionsSection)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
                $"{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)} is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
                $"{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)} is required.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.Secret) >= 32,
                $"{nameof(JwtOptions)}:{nameof(JwtOptions.Secret)} must contain at least 32 bytes.")
            .Validate(options => options.AccessTokenLifetimeMinutes is > 0 and <= 30,
                $"{nameof(JwtOptions)}:{nameof(JwtOptions.AccessTokenLifetimeMinutes)} must be between 1 and 30 minutes.")
            .Validate(options => options.RefreshTokenLifetime > 0,
                $"{nameof(JwtOptions)}:{nameof(JwtOptions.RefreshTokenLifetime)} must be greater than zero.")
            .ValidateOnStart();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = TokenValidationParametersFactory.Create(jwtOptions);
            });

        services.AddAuthorization();
        services.AddScoped<UserScopedData>();
        services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        
        return services;
    }
}
