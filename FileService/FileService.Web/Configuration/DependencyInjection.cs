using Core.Abstractions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Core.Features;
using FileService.Core.Features.AbordMultipartUpload;
using FileService.Core.Features.CancelMultipartUpload;
using FileService.Core.Features.CompleteMultipartUpload;
using FileService.Core.Features.Delete;
using FileService.Core.Features.GetDownloadUrl;
using FileService.Core.Features.GetMediaAssetInfo;
using FileService.Core.Features.GetMediaAssetsInfo;
using FileService.Core.Features.GetChunkUploadUrl;
using FileService.Core.Features.Upload;
using FluentValidation;
using FileService.Infrastructure;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.Repositories;
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

        services.AddScoped<IMediaAssetsRepository, MediaAssetsRepository>();

        services.AddEndpoints(typeof(UploadFileHandler).Assembly);

        services.AddS3(configuration);
        
        services.AddJwtAuthentication(configuration);

        services.AddScoped<IValidator<UploadFileCommand>, UploadFileValidator>();

        services.AddScoped<ICommandHandler<Guid, UploadFileCommand>, UploadFileHandler>();

        services.AddScoped<StartMultipartUploadHandler>();
        services.AddScoped<GetChunkUploadUrlHandler>();
        services.AddScoped<CompleteMultipartUploadHandler>();
        services.AddScoped<AbortMultipartUploadHandler>();
        services.AddScoped<CancelMultipartUploadHandler>();
        services.AddScoped<GetMediaAssetInfoHandler>();
        services.AddScoped<GetMediaAssetsInfoHandler>();
        services.AddScoped<GetDownloadUrlHandler>();
        services.AddScoped<DeleteFileHandler>();

        services.Configure<MultipartUploadOptions>(configuration.GetSection(nameof(S3Options)));

        return services;
    }
}
