using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Presentation.Configuration;

public static class DatabaseMigrationExtensions
{
    public static WebApplication UseDatabaseMigrations(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() &&
            !app.Configuration.GetValue<bool>("DatabaseMigration:Enabled"))
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            dbContext.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(exception, "Database migration failed. Application startup aborted.");
            throw;
        }

        return app;
    }
}
