using Core.Abstractions;
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
using FileService.Core.Processing;
using FluentValidation;
using FileService.Infrastructure;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.Repositories;
using FileService.VideoProcessing;
using FileService.VideoProcessing.Pipeline;
using FileService.VideoProcessing.Pipeline.Steps;
using FileService.VideoProcessing.ProcessRunner;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Jobs;
using Framework.Middleware;
using Framework.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Quartz;

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
        services.AddScoped<IVideoProcessingRepository, VideoProcessingRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<QuartzDbInitializer>();
        services.AddScoped<IProcessingPipeline, ProcessingPipeline>();
        services.AddScoped<FileService.VideoProcessing.VideoProcessingService>();
        services.AddScoped<IVideoProcessingService>(serviceProvider =>
            serviceProvider.GetRequiredService<FileService.VideoProcessing.VideoProcessingService>());
        services.AddScoped<IProcessRunner, ProcessRunner>();
        services.AddScoped<IFfmpegProcessRunner, FfmpegProcessRunner>();
        services.AddScoped<IProcessingStepHandler, InitializeStepHandler>();
        services.AddScoped<IProcessingStepHandler, ExtractMetadataStepHandler>();
        services.AddScoped<IProcessingStepHandler, GenerateHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, UploadHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, GeneratePreviewStepHandler>();
        services.AddScoped<IProcessingStepHandler, CleanupStepHandler>();
        services.AddScoped<VideoProcessingJobFactory>();
        services.AddScoped<IProcessingJobFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<VideoProcessingJobFactory>());
        services.AddScoped<ProcessingJobScheduler>();

        services.Configure<FileService.VideoProcessing.VideoProcessingOptions>(
            configuration.GetSection(FileService.VideoProcessing.VideoProcessingOptions.SectionName));

        services.AddQuartz(options =>
        {
            options.UsePersistentStore(storeOptions =>
            {
                storeOptions.UsePostgres(configuration.GetConnectionString("Database")!);
                storeOptions.UseNewtonsoftJsonSerializer();
                storeOptions.UseProperties = true;
            });
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

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

        var cacheOptions = configuration
                               .GetRequiredSection(CacheOptions.SectionName)
                               .Get<CacheOptions>()
                           ?? throw new InvalidOperationException(
                               $"{CacheOptions.SectionName} configuration is missing");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheOptions.RedisConnectionString;
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(cacheOptions.ExpirationTimeInMinutes),
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheOptions.LocalCacheExpiration)
            };
        });

        return services;
    }
}
