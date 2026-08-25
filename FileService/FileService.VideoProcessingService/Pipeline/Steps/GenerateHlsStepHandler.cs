using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class GenerateHlsStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_HLS;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            return Task.FromResult<Result<ProcessingContext, Error>>(
                Error.Failure(
                    "pipeline.working.directory.missing",
                    "Working directory must be initialized before generating HLS output."));
        }

        var updatedContext = context with
        {
            HlsOutputDirectory = Path.Combine(context.WorkingDirectory, "hls")
        };

        return Task.FromResult(Result.Success<ProcessingContext, Error>(updatedContext));
    }
}
