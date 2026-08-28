using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileService.Core;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public sealed class VideoProcessingJob(
    ILogger<VideoProcessingJob> logger,
    IVideoProcessingService videoProcessingService,
    IVideoProcessingRepository videoProcessingRepository,
    IMediaAssetsRepository mediaAssetsRepository,
    ITransactionManager transactionManager,
    VideoProcessingJobFactory jobFactory,
    IOptions<VideoProcessingOptions> options) : IJob
{
    public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap;
        Guid videoAssetId = dataMap.GetGuid(VideoAssetIdKey.Name);

        logger.LogInformation(
            "Starting video processing job for VideoAsset: {VideoAssetId}",
            videoAssetId);

        // FS-12: Ограничиваем время pipeline и одновременно уважаем остановку самого Quartz.
        using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            context.CancellationToken);
        timeoutTokenSource.CancelAfter(TimeSpan.FromMinutes(options.Value.ProcessingTimeoutMinutes));

        var result = await videoProcessingService.ProcessVideoAsync(videoAssetId, timeoutTokenSource.Token);

        if (result.IsFailure)
        {
            logger.LogError(
                "Video processing job failed for VideoAsset: {VideoAssetId}. Error: {Error}",
                videoAssetId,
                result.Error);

            // FS-12: Ошибка не перезапускается немедленно — создаётся контролируемый отложенный retry.
            await ScheduleRetryAsync(videoAssetId, context);

            throw new JobExecutionException(refireImmediately: false);
        }

        logger.LogInformation(
            "Video processing job completed for VideoAsset: {VideoAssetId}",
            videoAssetId);
    }

    private async Task ScheduleRetryAsync(Guid videoAssetId, IJobExecutionContext context)
    {
        // FS-12: Retry разрешает доменная модель; критические ошибки и исчерпанные попытки остаются FAILED.
        var processingResult = await videoProcessingRepository.GetByAsync(
            processing => processing.VideoAssetId == videoAssetId,
            CancellationToken.None);
        if (processingResult.IsFailure || !processingResult.Value.CanRetry())
            return;

        var assetResult = await mediaAssetsRepository.GetByIdAsync(videoAssetId, CancellationToken.None);
        if (assetResult.IsFailure)
            return;

        var processing = processingResult.Value;
        // FS-12: Каждая следующая попытка откладывается дольше предыдущей.
        var retryDelay = TimeSpan.FromSeconds(
            options.Value.RetryDelaySeconds * Math.Pow(2, processing.RetryCount));
        var nextRetryAt = DateTime.UtcNow.Add(retryDelay);

        var scheduleRetryResult = processing.ScheduleRetry(nextRetryAt);
        if (scheduleRetryResult.IsFailure)
        {
            logger.LogWarning(
                "Could not schedule retry for VideoAsset: {VideoAssetId}. Error: {Error}",
                videoAssetId,
                scheduleRetryResult.Error);
            return;
        }

        var saveResult = await transactionManager.SaveChangesAsync(CancellationToken.None);
        if (saveResult.IsFailure)
            return;

        var retryTrigger = jobFactory.CreateRetryTrigger(
            assetResult.Value,
            nextRetryAt,
            processing.RetryCount);

        await context.Scheduler.ScheduleJob(retryTrigger, CancellationToken.None);

        logger.LogInformation(
            "Scheduled retry {RetryCount} for VideoAsset: {VideoAssetId} at {NextRetryAt}",
            processing.RetryCount,
            videoAssetId,
            nextRetryAt);
    }
}
