using DirectoryService.Application.Auth;
using DirectoryService.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

internal sealed class AuthUsersSynchronizationBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AuthUsersSynchronizationOptions> options,
    TimeProvider timeProvider,
    ILogger<AuthUsersSynchronizationBackgroundService> logger)
    : BackgroundService
{
    private readonly AuthUsersSynchronizationOptions _options =
        options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await SynchronizeAsync(stoppingToken);

        using var timer = new PeriodicTimer(
            TimeSpan.FromMinutes(_options.IntervalMinutes),
            timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SynchronizeAsync(stoppingToken);
        }
    }

    private async Task SynchronizeAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var authServiceClient = scope.ServiceProvider
                .GetRequiredService<IAuthServiceClient>();

            var result = await authServiceClient.GetUsersAsync(
                page: 1,
                pageSize: _options.PageSize,
                cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Auth users synchronization failed with error {ErrorCode}",
                    result.Error.Code);
                return;
            }

            logger.LogInformation(
                "Auth users synchronization read {UserCount} of {TotalCount} users",
                result.Value.Items.Count,
                result.Value.TotalCount);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Auth users synchronization failed");
        }
    }
}
