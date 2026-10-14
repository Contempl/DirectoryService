using AuthService.Application;
using AuthService.Application.Database;
using AuthService.Application.Features.Account.AddRoles;
using AuthService.Application.Features.Account.ChangePassword;
using AuthService.Application.Features.Account.Deactivate;
using AuthService.Application.Features.Account.GetPagedUsers;
using AuthService.Application.Features.Account.Profile;
using AuthService.Application.Features.Account.RemoveRoles;
using AuthService.Application.Features.ConfirmEmail;
using AuthService.Application.Features.Login;
using AuthService.Application.Features.Register;
using AuthService.Application.Features.ResendConfirmation;
using AuthService.Application.Features.ResetPassword;
using AuthService.Application.Features.SendPassResetLink;
using AuthService.Core;
using AuthService.Core.Authorization;
using AuthService.Core.Database;
using AuthService.Core.Identity;
using AuthService.Core.Options;
using AuthService.Core.Repositories;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using FluentValidation;
using Framework.Response;
using AuthService.Domain.Authorization;
using Core.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using Shared.Kernel;
using UserScopedData = AuthService.Application.Auth.UserScopedData;

namespace AuthService.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var authorizationServerSection = configuration.GetRequiredSection(
            AuthorizationServerOptions.SectionName);

        services.AddOptions<AuthorizationServerOptions>()
            .Bind(authorizationServerSection)
            .Validate(options => Uri.TryCreate(options.Issuer, UriKind.Absolute, out _),
                $"{AuthorizationServerOptions.SectionName}:Issuer must be an absolute URI.")
            .Validate(options => Uri.TryCreate(options.LoginPageUrl, UriKind.Absolute, out _),
                $"{AuthorizationServerOptions.SectionName}:LoginPageUrl must be an absolute URI.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiScope),
                $"{AuthorizationServerOptions.SectionName}:ApiScope is required.")
            .Validate(options => options.AccessTokenLifetimeMinutes is > 0 and <= 30,
                $"{AuthorizationServerOptions.SectionName}:AccessTokenLifetimeMinutes must be between 1 and 30 minutes.")
            .Validate(options => options.RefreshTokenLifetimeDays > 0,
                $"{AuthorizationServerOptions.SectionName}:RefreshTokenLifetimeDays must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<OpenIddictClientsOptions>()
            .Bind(configuration.GetRequiredSection(
                OpenIddictClientsOptions.SectionName))
            .Validate(options => IsValidConfidentialClient(options.Frontend),
                $"{OpenIddictClientsOptions.SectionName}:Frontend client configuration is incomplete.")
            .Validate(options => Uri.TryCreate(
                    options.Frontend.RedirectUri,
                    UriKind.Absolute,
                    out _),
                $"{OpenIddictClientsOptions.SectionName}:Frontend:RedirectUri must be an absolute URI.")
            .Validate(options => IsValidConfidentialClient(options.DirectoryService),
                $"{OpenIddictClientsOptions.SectionName}:DirectoryService client configuration is incomplete.")
            .Validate(options =>
                    options.DirectoryService.SubjectId is { } subjectId &&
                    subjectId != Guid.Empty,
                $"{OpenIddictClientsOptions.SectionName}:DirectoryService:SubjectId is required.")
            .ValidateOnStart();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.Zero);

        services.AddHttpContextAccessor();

        services.AddScoped<UserScopedData>();
        services.AddSingleton<IRolePermissionResolver, AuthRolePermissionResolver>();
        services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthorizationSessionRevoker,
            OpenIddictAuthorizationSessionRevoker>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();
        services.AddScoped<IValidator<SendResetPasswordRequest>, SendResetPasswordValidator>();
        services.AddScoped<IValidator<ResendConfirmationRequest>, ResendConfirmationValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();
        services.AddScoped<IValidator<AssignRoleRequest>, AssignRoleValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordValidator>();

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<SendResetPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ResendConfirmationHandler>();
        services.AddScoped<AssignRoleHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetProfileHandler>();
        services.AddScoped<RemoveRolesHandler>();
        services.AddScoped<IQueryHandler<ConfirmEmailQuery, UnitResult<Error>>, ConfirmEmailQueryHandler>();

        services.AddEndpoints(typeof(RegisterEndpoint).Assembly);


        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme(
                JwtBearerDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme,
                options => options.ForwardDefault =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "directory.provider";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                var serverOptions = authorizationServerSection
                    .Get<AuthorizationServerOptions>()
                    ?? throw new InvalidOperationException(
                        $"Configuration section '{AuthorizationServerOptions.SectionName}' is invalid.");

                options.SetIssuer(new Uri(serverOptions.Issuer, UriKind.Absolute));
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetUserInfoEndpointUris("/connect/userinfo");

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Email,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Roles,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.OfflineAccess,
                    serverOptions.ApiScope);

                options.SetAccessTokenLifetime(
                    TimeSpan.FromMinutes(serverOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(
                    TimeSpan.FromDays(serverOptions.RefreshTokenLifetimeDays));

                options.DisableAccessTokenEncryption();

                if (environment.IsDevelopment() || environment.IsEnvironment("Docker"))
                {
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();
                }
                else
                {
                    AddProductionCertificates(options, serverOptions);
                }

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();

                if (environment.IsDevelopment() || environment.IsEnvironment("Docker"))
                    aspNetCore.DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddDatabaseWithLogging(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .UseOpenIddict()
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

        services.Configure<AdminOptions>(
            configuration.GetSection(nameof(AdminOptions))
        );

        services.Configure<SmtpOptions>(
            configuration.GetSection(nameof(SmtpOptions))
        );

        return services;
    }

    private static void AddProductionCertificates(
        OpenIddictServerBuilder builder,
        AuthorizationServerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningCertificatePath) ||
            string.IsNullOrWhiteSpace(options.EncryptionCertificatePath))
        {
            throw new InvalidOperationException(
                "Production OpenIddict signing and encryption certificates are not configured.");
        }

        builder.AddSigningCertificate(
            System.Security.Cryptography.X509Certificates.X509CertificateLoader
                .LoadPkcs12FromFile(
                    options.SigningCertificatePath,
                    options.SigningCertificatePassword));
        builder.AddEncryptionCertificate(
            System.Security.Cryptography.X509Certificates.X509CertificateLoader
                .LoadPkcs12FromFile(
                    options.EncryptionCertificatePath,
                    options.EncryptionCertificatePassword));
    }

    private static bool IsValidConfidentialClient(
        OpenIddictClientOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.ClientId) &&
               !string.IsNullOrWhiteSpace(options.ClientSecret) &&
               !string.IsNullOrWhiteSpace(options.DisplayName);
    }
}
