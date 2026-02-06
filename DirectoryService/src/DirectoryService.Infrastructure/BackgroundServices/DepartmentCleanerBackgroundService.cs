using Dapper;
using DirectoryService.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;    
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class DepartmentCleanerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BackgroundServiceOptions _options;
    private readonly ILogger<DepartmentCleanerBackgroundService> _logger;

    public DepartmentCleanerBackgroundService(
        ILogger<DepartmentCleanerBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<BackgroundServiceOptions> options)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var period = TimeSpan.FromHours(_options.DepartmentCleanupIntervalHours);

        _logger.LogInformation("Cleaner started. Period: {Hours}h.", 
            _options.DepartmentCleanupIntervalHours );
        
        _logger.LogInformation("Department Cleanup Worker started.");

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
        var thresholdMonths = _options.HardDeleteThresholdMonths;
        _logger.LogInformation("Starting department cleanup process...");

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-thresholdMonths);
            _logger.LogInformation("Cleaning up departments deactivated before {CutoffDate}", cutoffDate);

            var departmentsToDelete = await dbContext.Departments
                .Where(d => !d.IsActive && d.DeletedAt <= cutoffDate)
                .Select(d => new { d.Id, d.Name, d.DeletedAt })
                .ToListAsync(cancellationToken);

            if (!departmentsToDelete.Any())
            {
                _logger.LogInformation("No departments to delete");
                return;
            }

            _logger.LogInformation("Found {Count} departments to delete: {Departments}",
                departmentsToDelete.Count,
                string.Join(", ", departmentsToDelete.Select(d => $"{d.Name} (ID: {d.Id})")));
            
            var sql = """
                      WITH delete_departments AS (
                          DELETE FROM departments
                          WHERE is_active = false AND deleted_at <= @cutoffDate
                          RETURNING id, parent_id, path, depth
                      ),
                      delete_join_position AS (
                          DELETE FROM departments_positions
                          USING delete_departments
                          WHERE department_id = delete_departments.id
                      ),
                      delete_join_location AS (
                          DELETE FROM departments_locations
                          USING delete_departments
                          WHERE department_id = delete_departments.id
                      ),
                      update_child AS (
                          UPDATE departments depart SET 
                              parent_id = delete_departments.parent_id,
                              path = (subpath(delete_departments.path, 0, -1) || depart.name::ltree),
                              depth = depart.depth - 1
                          FROM delete_departments
                          WHERE depart.parent_id = delete_departments.id
                          RETURNING depart.id, depart.path
                      )
                      UPDATE departments dep SET
                          path = (subpath(update_child.path, 0, -1) || subpath(dep.path, delete_departments.depth)),
                          depth = dep.depth - 1
                      FROM update_child, delete_departments
                      WHERE dep.path <@ delete_departments.path 
                          AND dep.id != delete_departments.id 
                          AND dep.id != update_child.id
                      """;

            var connection = dbContext.Database.GetDbConnection();
            var result = await connection.ExecuteAsync(
                sql,
                new { cutoffDate },
                transaction: transaction.GetDbTransaction());

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully deleted {Count} departments and updated their children hierarchy",
                departmentsToDelete.Count
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogError(ex, "Failed to cleanup departments. Transaction rolled back.");
        }
    }
}