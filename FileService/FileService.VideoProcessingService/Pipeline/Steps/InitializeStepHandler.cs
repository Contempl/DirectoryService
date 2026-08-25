using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class InitializeStepHandler : IProcessingStepHandler
{
    private readonly ILogger<InitializeStepHandler> _logger;

    public InitializeStepHandler(ILogger<InitializeStepHandler> logger)
    {
        _logger = logger;
    }

    public StepType StepType => StepType.INITIALIZE;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Initializing video processing for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        var createDirectoryResult = context.CreateWorkingDirectory();
        if (createDirectoryResult.IsFailure)
            return Task.FromResult<Result<ProcessingContext, Error>>(createDirectoryResult.Error);

        return Task.FromResult(Result.Success<ProcessingContext, Error>(context));
    }
}
