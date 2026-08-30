using FileService.Infrastructure.Postgres;
using Framework.Middleware;
using Framework.Response;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FileService.Configuration;

public static class ApiExtensions
{
    public static IApplicationBuilder ConfigureApp(this WebApplication app)
    {
        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
            try
            {
                dbContext.Database.Migrate();
                Log.Information("Миграции успешно применены.");
                Console.WriteLine("Миграции успешно применены.");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error occured while applying migrations. {ex.Message}");
                Console.WriteLine($"Error occured while applying migrations. {ex.Message}");
                throw;
            }
        }

        app.UseExceptionHandlingMiddleware();

        app.UseCors(bld =>
        {
            bld.WithOrigins("http://localhost:3000",
                    "http://localhost:8002")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        
        app.UseAuthentication();
        app.UseDevAuth();
        app.UseMiddleware<UserScopedDataMiddleware>();
        app.UseAuthorization();

        var apiGroup = app.MapGroup("/api").WithOpenApi();
        app.MapEndpoints(apiGroup);

        return app;
    }
}
