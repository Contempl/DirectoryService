using Core.Auth;
using Framework.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace DirectoryService.Presentation.Authorization;

public static class IdentityProviderExtensions
{
    public static IServiceCollection AddIdentityProviderAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(
            IdentityProviderOptions.SectionName);
        var identityProvider = section.Get<IdentityProviderOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{IdentityProviderOptions.SectionName}' is invalid.");
        var normalizedIssuer = new Uri(
            identityProvider.Authority,
            UriKind.Absolute).AbsoluteUri;

        services.AddOptions<IdentityProviderOptions>()
            .Bind(section)
            .Validate(options => Uri.TryCreate(
                    options.Authority,
                    UriKind.Absolute,
                    out _),
                $"{IdentityProviderOptions.SectionName}:Authority must be an absolute URI.")
            .Validate(options => Uri.TryCreate(
                    options.MetadataAddress,
                    UriKind.Absolute,
                    out _),
                $"{IdentityProviderOptions.SectionName}:MetadataAddress must be an absolute URI.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
                $"{IdentityProviderOptions.SectionName}:Audience is required.")
            .ValidateOnStart();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityProvider.Authority;
                options.MetadataAddress = identityProvider.MetadataAddress;
                options.Audience = identityProvider.Audience;
                options.RequireHttpsMetadata =
                    identityProvider.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = normalizedIssuer,
                    ValidateAudience = true,
                    ValidAudience = identityProvider.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();
        services.AddScoped<UserScopedData>();
        services.AddScoped<IAuthorizationHandler,
            PermissionRequirementHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider,
            PermissionPolicyProvider>();

        return services;
    }
}
