using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline;

public interface IProcessingPipeline
{
    Task<UnitResult<Error>> ProcessAllStepsAsync(Guid  videoAssetId, CancellationToken cancellationToken = default);
}