using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class InitializeStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.INITIALIZE;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workingDirectory = Path.Combine(
            Path.GetTempPath(),
            "file-service",
            context.VideoAsset.Id.ToString());

        var updatedContext = context with
        {
            WorkingDirectory = workingDirectory
        };

        return Task.FromResult(Result.Success<ProcessingContext, Error>(updatedContext));
    }
}
