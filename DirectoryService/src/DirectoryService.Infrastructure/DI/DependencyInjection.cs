using DirectoryService.Application.Auth;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Auth;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Options;
using DirectoryService.Infrastructure.Repositories;
using FileService.Contracts.HttpCommunication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace DirectoryService.Infrastructure.DI;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var authServiceOptionsSection = configuration
            .GetRequiredSection(AuthServiceOptions.SectionName);
        var authServiceOptions = authServiceOptionsSection
            .Get<AuthServiceOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{AuthServiceOptions.SectionName}' is invalid.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));
        services.AddDbContext<IReadDbContext, ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));
        services.AddHostedService<DepartmentCleanerBackgroundService>();
        services.AddHostedService<AuthUsersSynchronizationBackgroundService>();
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.Configure<BackgroundServiceOptions>(
            configuration.GetSection(BackgroundServiceOptions.SectionName));

        services.AddOptions<AuthServiceOptions>()
            .Bind(authServiceOptionsSection)
            .Validate(
                options => Uri.TryCreate(
                    options.BaseAddress,
                    UriKind.Absolute,
                    out _),
                $"{AuthServiceOptions.SectionName}:BaseAddress must be an absolute URI.")
            .Validate(
                options => options.TimeoutSeconds is > 0 and <= 60,
                $"{AuthServiceOptions.SectionName}:TimeoutSeconds must be between 1 and 60 seconds.")
            .ValidateOnStart();

        services.AddOptions<ServiceTokenOptions>()
            .Bind(configuration.GetRequiredSection(
                ServiceTokenOptions.SectionName))
            .Validate(
                options => options.SubjectId != Guid.Empty,
                $"{ServiceTokenOptions.SectionName}:SubjectId is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceName),
                $"{ServiceTokenOptions.SectionName}:ServiceName is required.")
            .Validate(
                options => options.LifetimeMinutes is > 0 and <= 15,
                $"{ServiceTokenOptions.SectionName}:LifetimeMinutes must be between 1 and 15 minutes.")
            .Validate(
                options => options.RefreshBeforeExpirationSeconds >= 0 &&
                           options.RefreshBeforeExpirationSeconds <
                           options.LifetimeMinutes * 60,
                $"{ServiceTokenOptions.SectionName}:RefreshBeforeExpirationSeconds must be shorter than the token lifetime.")
            .ValidateOnStart();

        services.AddOptions<AuthUsersSynchronizationOptions>()
            .Bind(configuration.GetRequiredSection(
                AuthUsersSynchronizationOptions.SectionName))
            .Validate(
                options => !options.Enabled ||
                           options.IntervalMinutes is > 0 and <= 1440,
                $"{AuthUsersSynchronizationOptions.SectionName}:IntervalMinutes must be between 1 and 1440 minutes.")
            .Validate(
                options => !options.Enabled ||
                           options.PageSize is > 0 and <= 100,
                $"{AuthUsersSynchronizationOptions.SectionName}:PageSize must be between 1 and 100.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IServiceTokenProvider, ServiceTokenProvider>();
        services.AddTransient<AuthServiceAuthorizationHandler>();

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<AuthServiceOptions>>()
                        .Value;

                    client.BaseAddress = new Uri(
                        options.BaseAddress,
                        UriKind.Absolute);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                })
            .AddHttpMessageHandler<AuthServiceAuthorizationHandler>()
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(
                        authServiceOptions.TimeoutSeconds);

                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(200);
                options.Retry.BackoffType =
                    DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.Retry.DisableForUnsafeHttpMethods();

                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(30);
                options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(15);

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(2);
            });

        services.AddFileServiceHttpCommunication(configuration);
    }
}
