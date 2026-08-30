using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;
using VideoProcessingEntity = FileService.Domain.MediaProcessing.VideoProcessing;

namespace FileService.VideoProcessing.Pipeline;

public class ProcessingPipeline : IProcessingPipeline
{
    private readonly IEnumerable<IProcessingStepHandler> _stepHandlers;
    private readonly ILogger<ProcessingPipeline> _logger;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly ITransactionManager _transactionManager;

    public ProcessingPipeline(IEnumerable<IProcessingStepHandler> stepHandlers,
        ILogger<ProcessingPipeline> logger,
        IVideoProcessingRepository videoProcessingRepository,
        IMediaAssetsRepository mediaAssetsRepository,
        ITransactionManager transactionManager)
    {
        _stepHandlers = stepHandlers;
        _logger = logger;
        _videoProcessingRepository = videoProcessingRepository;
        _mediaAssetsRepository = mediaAssetsRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> ProcessAllStepsAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        var contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
        if (contextResult.IsFailure)
            return contextResult.Error;

        var context = contextResult.Value;

        var executionResult = await ExecuteAllStepsAsync(context, cancellationToken);
        if (executionResult.IsFailure)
            return await FinalizeWithFailureAsync(context, executionResult.Error, cancellationToken);

        return await FinalizeAsync(context, cancellationToken);
    }

    private async Task<UnitResult<Error>> ExecuteAllStepsAsync(
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var stepResult = context.VideoProcessing.ProcessNextStep();

            if (stepResult.IsFailure)
                return stepResult.Error;

            if (stepResult.Value is null)
                return UnitResult.Success<Error>();

            var currentStep = stepResult.Value;

            // FS-13: Сохраняем PROCESSING до долгого handler-а, чтобы GET и SSE увидели текущий шаг.
            var startStepSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (startStepSaveResult.IsFailure)
                return startStepSaveResult.Error;

            var stepHandler = _stepHandlers.FirstOrDefault(s => s.StepType == currentStep.Type);
            if (stepHandler is null)
            {
                return Error.Failure(
                    "pipeline.handler.not.found",
                    $"No handler found for step type {currentStep.Type}");
            }

            var executeResult = await ExecuteStepSafelyAsync(
                stepHandler,
                context,
                cancellationToken);

            if (executeResult.IsFailure)
                return executeResult.Error;

            context = executeResult.Value;

            var completeResult = context.VideoProcessing.CompleteCurrentStep();
            if (completeResult.IsFailure)
                return completeResult.Error;

            var stepSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (stepSaveResult.IsFailure)
                return stepSaveResult.Error;
        }
    }

    private async Task<UnitResult<Error>> FinalizeAsync(
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        var assetCompleteResult = context.VideoAsset.CompleteProcessing();
        if (assetCompleteResult.IsFailure)
            return assetCompleteResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogInformation(
            "Video processing completed for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        return UnitResult.Success<Error>();
    }

    private async Task<UnitResult<Error>> FinalizeWithFailureAsync(
        ProcessingContext context,
        Error error,
        CancellationToken cancellationToken)
    {
        var processingFailResult = context.VideoProcessing.CurrentStep is not null
            ? context.VideoProcessing.FailCurrentStep(error.Message)
            : context.VideoProcessing.Fail(error.Message);

        if (processingFailResult.IsFailure)
            return processingFailResult.Error;

        var assetFailResult = context.VideoAsset.MarkFailed();
        if (assetFailResult.IsFailure)
            return assetFailResult.Error;

        CleanupLocalWorkspace(context);

        var saveCancellationToken = cancellationToken.IsCancellationRequested
            ? CancellationToken.None
            : cancellationToken;

        var saveResult = await _transactionManager.SaveChangesAsync(saveCancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogError(
            "Video processing failed for VideoAssetId: {VideoAssetId}. Error: {Error}",
            context.VideoAsset.Id,
            error.Message);

        return error;
    }

    private void CleanupLocalWorkspace(ProcessingContext context)
    {
        string? workingDirectory = context.WorkingDirectory;
        if (string.IsNullOrWhiteSpace(workingDirectory))
            return;

        try
        {
            if (Directory.Exists(workingDirectory))
                Directory.Delete(workingDirectory, recursive: true);

            context.Cleanup();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to clean local workspace {WorkingDirectory} for VideoAssetId: {VideoAssetId}",
                workingDirectory,
                context.VideoAsset.Id);
        }
    }

    private async Task<Result<ProcessingContext, Error>> ExecuteStepSafelyAsync(
        IProcessingStepHandler handler,
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        // FS-12: Каждый background-step логирует asset, название шага и фактическую длительность.
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            return await handler.ExecuteAsync(context, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Video processing step {StepType} was cancelled",
                handler.StepType);

            return Error.Failure(
                "pipeline.step.cancelled",
                "Video processing was cancelled.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled error while executing video processing step {StepType}",
                handler.StepType);

            return Error.Failure(
                "pipeline.step.execution.failed",
                exception.Message);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Video processing step {StepType} finished for VideoAssetId: {VideoAssetId} in {DurationMs} ms",
                handler.StepType,
                context.VideoAsset.Id,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<Result<ProcessingContext, Error>> LoadContextAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        var processingResult = await _videoProcessingRepository
            .GetByAsync(vp => vp.VideoAssetId == videoAssetId, cancellationToken);

        VideoProcessingEntity videoProcess;

        if (processingResult.IsFailure)
        {
            var newProcess = new VideoProcessingEntity(videoAssetId);
            videoProcess = newProcess;
            
            _videoProcessingRepository.Add(videoProcess);
            
            _logger.LogInformation("Created video process for VideoAsset: {VideoAssetId}", videoAssetId);
        }
        else
        {
            videoProcess = processingResult.Value;
            _logger.LogInformation("Loaded existing VideoProcess for VideoAsset: {VideoAssetId}", videoAssetId);
        }

        var assetResult = await _mediaAssetsRepository
            .GetVideoBy(a => a.Id == videoAssetId, cancellationToken);
        
        if (assetResult.IsFailure)
            return assetResult.Error;

        // FS-12: FAILED processing можно перезапустить только по наступившему Quartz retry.
        if (videoProcess.Status == ProcessingStatus.FAILED)
        {
            if (videoProcess.NextRetryAt is null || videoProcess.NextRetryAt > DateTime.UtcNow)
            {
                return Error.Validation(
                    "processing.retry.not.scheduled",
                    "Failed processing can only restart when its scheduled retry is due");
            }

            var prepareRetryResult = assetResult.Value.PrepareProcessingRetry();
            if (prepareRetryResult.IsFailure)
                return prepareRetryResult.Error;

            var resetResult = videoProcess.Reset();
            if (resetResult.IsFailure)
                return resetResult.Error;
        }

        var startResult = assetResult.Value.StartProcessing();
        if (startResult.IsFailure)
            return startResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        ProcessingContext processingContext = new ProcessingContext
        {
            VideoAsset = assetResult.Value,
            VideoProcessing = videoProcess
        };

        return processingContext;
    }
}
