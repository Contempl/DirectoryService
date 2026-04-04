using AuthService.Application;
using AuthService.Application.Abstractions;
using AuthService.Application.Database;
using AuthService.Application.Factories;
using AuthService.Application.Features.ConfirmEmail;
using AuthService.Application.Features.Login;
using AuthService.Application.Features.Logout;
using AuthService.Application.Features.RefreshToken;
using AuthService.Application.Features.Register;
using AuthService.Application.Features.ResetPassword;
using AuthService.Application.Features.SendPassResetLink;
using AuthService.Core;
using AuthService.Core.Database;
using AuthService.Core.Identity;
using AuthService.Core.Options;
using AuthService.Core.Repositories;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using FluentValidation;
using Framework.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Kernel;

namespace AuthService.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddHostedService<DeleteRevokedTokensService>();

        services.AddHttpContextAccessor();

        services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();
        services.AddScoped<IValidator<SendResetPasswordRequest>, SendResetPasswordValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();

        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<SendResetPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<IQueryHandler<ConfirmEmailQuery, UnitResult<Error>>, ConfirmEmailQueryHandler>();

        services.AddEndpoints(typeof(RegisterEndpoint).Assembly);


        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
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

        return services;
    }

    public static IServiceCollection AddDatabaseWithLogging(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

        services.Configure<AdminOptions>(
            configuration.GetSection(nameof(AdminOptions))
        );

        services.Configure<JwtOptions>(
            configuration.GetSection(nameof(JwtOptions))
        );

        services.AddSingleton<IJwtOptions>(sp =>
            sp.GetRequiredService<IOptions<JwtOptions>>().Value);

        services.Configure<SmtpOptions>(
            configuration.GetSection(nameof(SmtpOptions))
        );

        services.Configure<BackgroundServiceOptions>(
            configuration.GetSection(nameof(BackgroundServiceOptions))
        );

        return services;
    }
}