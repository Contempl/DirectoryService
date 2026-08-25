using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class GeneratePreviewStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_PREVIEW;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(Result.Success<ProcessingContext, Error>(context));
    }
}
