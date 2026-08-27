using FileService.Core.Processing;
using FileService.Domain.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public sealed class VideoProcessingJobFactory : IProcessingJobFactory
{
    private const string JobGroup = "video-processing";

    public bool CanProcess(MediaAsset mediaAsset)
    {
        return mediaAsset is VideoAsset;
    }

    public IJobDetail CreateJob(MediaAsset mediaAsset)
    {
        return JobBuilder.Create<VideoProcessingJob>()
            .WithIdentity($"video-processing-{mediaAsset.Id}", JobGroup)
            .UsingJobData(VideoProcessingJob.VideoAssetIdKey.Name, mediaAsset.Id.ToString())
            // FS-12: Quartz повторно запустит job после аварийного завершения процесса приложения.
            .RequestRecovery()
            .StoreDurably(false)
            .Build();
    }

    public ITrigger CreateTrigger(MediaAsset mediaAsset)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-trigger-{mediaAsset.Id}", JobGroup)
            .StartNow()
            .Build();
    }

    public ITrigger CreateRetryTrigger(MediaAsset mediaAsset, DateTime startAtUtc, int retryCount)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-retry-{mediaAsset.Id}-{retryCount}", JobGroup)
            .ForJob($"video-processing-{mediaAsset.Id}", JobGroup)
            // FS-12: Retry запускается позже, а не через refireImmediately.
            .StartAt(startAtUtc)
            .Build();
    }
}
