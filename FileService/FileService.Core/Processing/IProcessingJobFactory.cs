using FileService.Domain.Assets;
using Quartz;

namespace FileService.Core.Processing;

public interface IProcessingJobFactory
{
    bool CanProcess(MediaAsset mediaAsset);

    IJobDetail CreateJob(MediaAsset mediaAsset);

    ITrigger CreateTrigger(MediaAsset mediaAsset);

    ITrigger CreateRetryTrigger(MediaAsset mediaAsset, DateTime startAtUtc, int retryCount);
}
