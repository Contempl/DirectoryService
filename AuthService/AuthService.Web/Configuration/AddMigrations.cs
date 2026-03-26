using AuthService.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AuthService.Configuration;

public static class AddMigrations
{
    public static IApplicationBuilder UseMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            try
            {
                dbContext.Database.Migrate();
                Log.Information("Migrations applied successfully.");
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error occured while applying migrations. {ex.Message}");
                Console.WriteLine($"Error occured while applying migrations. {ex.Message}");
                throw;
            }
        }

        return app;
    }
}