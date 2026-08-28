using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Microsoft.Extensions.Logging;
using Quartz;
using Shared.Kernel;

namespace FileService.Core.Processing;

public sealed class ProcessingJobScheduler(
    ISchedulerFactory schedulerFactory,
    IEnumerable<IProcessingJobFactory> processingJobFactories,
    ILogger<ProcessingJobScheduler> logger)
{
    // FS-12: Общая точка постановки media processing job для обычной и multipart-загрузки.
    public async Task<UnitResult<Error>> ScheduleAsync(
        MediaAsset mediaAsset,
        CancellationToken cancellationToken = default)
    {
        var factory = processingJobFactories.FirstOrDefault(f => f.CanProcess(mediaAsset));
        if (factory is null)
        {
            logger.LogError("No processing job factory for MediaAsset: {MediaAssetId}", mediaAsset.Id);
            return GeneralErrors.Failure();
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var job = factory.CreateJob(mediaAsset);

        // FS-12: Одинаковый JobKey не позволяет повторному HTTP-запросу создать дубликат job.
        if (await scheduler.CheckExists(job.Key, cancellationToken))
        {
            logger.LogInformation(
                "Processing job already exists for MediaAsset: {MediaAssetId}",
                mediaAsset.Id);
            return UnitResult.Success<Error>();
        }

        try
        {
            await scheduler.ScheduleJob(
                job,
                factory.CreateTrigger(mediaAsset),
                cancellationToken);
        }
        // FS-12: Между CheckExists и ScheduleJob возможна гонка двух запросов.
        catch (ObjectAlreadyExistsException)
        {
            logger.LogInformation(
                "Processing job was scheduled concurrently for MediaAsset: {MediaAssetId}",
                mediaAsset.Id);
            return UnitResult.Success<Error>();
        }

        logger.LogInformation("Scheduled processing job for MediaAsset: {MediaAssetId}", mediaAsset.Id);
        return UnitResult.Success<Error>();
    }
}
