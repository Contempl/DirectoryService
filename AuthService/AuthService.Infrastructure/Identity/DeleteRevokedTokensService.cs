using AuthService.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Core.Identity;

public class DeleteRevokedTokensService : BackgroundService
{
    private readonly ILogger<DeleteRevokedTokensService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BackgroundServiceOptions _options;

    public DeleteRevokedTokensService(
        IOptions<BackgroundServiceOptions> options, 
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DeleteRevokedTokensService> logger)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var period = TimeSpan.FromHours(_options.RemoveRevokedTokenIntervalHours);
        
        _logger.LogInformation("Removing revoked tokens.");
        
        using var timer = new PeriodicTimer(period);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                await ProcessCleanup(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cleanup failed");
            }
        }
    }

    private async Task ProcessCleanup(CancellationToken cancellationToken)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-_options.ThresholdTokensDays);
        
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var refreshTokensToDelete = await dbContext.RefreshTokens
                .Where(rt => rt.IsRevoked && rt.RevokedAt < thresholdDate)
                .ToListAsync(cancellationToken: cancellationToken);

            if (!refreshTokensToDelete.Any())
            {
                _logger.LogInformation("No tokens to delete to delete");
                return;
            }

            _logger.LogInformation("Found {Count} tokens to delete.",
                refreshTokensToDelete.Count);

            dbContext.RefreshTokens.RemoveRange(refreshTokensToDelete);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogError(ex, "Cleanup failed. Transaction rolled back.");
        }
    }
}