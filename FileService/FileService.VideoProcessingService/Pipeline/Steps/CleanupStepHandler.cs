using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class CleanupStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.CLEANUP;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var updatedContext = context with
        {
            WorkingDirectory = null,
            HlsOutputDirectory = null
        };

        return Task.FromResult(Result.Success<ProcessingContext, Error>(updatedContext));
    }
}
