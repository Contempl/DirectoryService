using CSharpFunctionalExtensions;
using FileService.Domain;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class ExtractMetadataStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.EXTRACT_METADATA;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadataResult = VideoMetadata.Create(
            TimeSpan.FromSeconds(1),
            width: 1920,
            height: 1080);

        if (metadataResult.IsFailure)
            return Task.FromResult<Result<ProcessingContext, Error>>(metadataResult.Error);

        context.VideoAsset.SetMetadata(metadataResult.Value);

        return Task.FromResult(Result.Success<ProcessingContext, Error>(context));
    }
}
