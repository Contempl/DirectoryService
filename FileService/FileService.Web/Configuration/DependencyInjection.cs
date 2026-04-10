using Core.Abstractions;
using CSharpFunctionalExtensions;
using FileService.Core.Features.Delete;
using FileService.Core.Features.Download;
using FileService.Core.Features.Upload;
using FileService.Infrastructure;
using FileService.Infrastructure.Postgres;
using Framework.Middleware;
using Framework.Response;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;

namespace FileService.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FileServiceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

        services.AddEndpoints(typeof(EndpointsExtensions).Assembly);
        
        services.AddS3(configuration);
        
        services.AddJwtAuthentication(configuration);

        services.AddScoped<ICommandHandler<Guid, DeleteFileRequest>, DeleteFileHandler>();
        services.AddScoped<ICommandHandler<Guid, UploadFileCommand>, UploadFileHandler>();
        services.AddScoped<IQueryHandler<DownloadFileRequest, Result<string, Error>>, DownloadFileHandler>();
        
        return services;
    }
}