using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.MediaProcessing;

public class VideoProcessing
{
    private static readonly Dictionary<StepType, int> _stepWeights = new()
    {
        { StepType.INITIALIZE, 0},
        { StepType.EXTRACT_METADATA, 10 },
        { StepType.GENERATE_HLS, 60 },
        { StepType.UPLOAD_HLS, 15 },
        { StepType.GENERATE_PREVIEW, 10 },
        { StepType.CLEANUP, 5 },
    };
    
    private readonly List<ProcessingStep> _steps = [];
    
    public Guid Id { get; private set; }
    
    public Guid VideoAssetId { get; private set; }

    public ProcessingStatus Status { get; private set; }

    public int ProgressPercentage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool IsCriticalError { get; private set; }

    public int RetryCount { get; private set; }

    public int MaxRetries { get; private set; } = 3;

    public DateTime? NextRetryAt { get; private set; }
    
    public DateTime? StartedAt { get; private set; }
    
    public DateTime? CompletedAt { get; private set; }
    
    public IReadOnlyList<ProcessingStep> Steps => _steps;

    public ProcessingStep? CurrentStep => _steps.FirstOrDefault(s => s.Status == StepStatus.PROCESSING);
    
    private VideoProcessing() {}

    public VideoProcessing(Guid videoAssetId)
    {
        Id = Guid.NewGuid();
        VideoAssetId = videoAssetId;
        Status = ProcessingStatus.PROCESSING;
        ProgressPercentage = 0;
        StartedAt = DateTime.UtcNow;

        InitializeSteps();
    }

    

    private void InitializeSteps()
    {
        int order = 1;
        foreach ((StepType stepType, int weight) in _stepWeights)
        {
            _steps.Add(new ProcessingStep(stepType, order++, weight));
        }
    }

    public Result<ProcessingStep?, Error> ProcessNextStep()
    {
        if (Status != ProcessingStatus.PROCESSING)
        {
            return Error.Failure("processing.invalid.status",
                $"Cannot process step when status is {Status}");
        }

        ProcessingStep? currentStep = CurrentStep;
        if (currentStep is not null)
            return currentStep;
        
        var nextStep = _steps
            .OrderBy(s => s.Order)
            .FirstOrDefault(s => s.Status == StepStatus.PENDING);

        if (nextStep is null)
        {
            UnitResult<Error> completeResult = Complete();
            if (completeResult.IsFailure)
                return completeResult.Error;

            return Result.Success<ProcessingStep?, Error>(null);
        }
        
        UnitResult<Error> startResult = nextStep.Start();
        if (startResult.IsFailure)
            return startResult.Error;

        RecalculateProgress();

        return nextStep;
    }

    private void RecalculateProgress()
    {
        int totalProgress = _steps
            .Where(s => s.Status == StepStatus.COMPLETED)
            .Sum(s => s.Weight);

        ProgressPercentage = totalProgress;
    }

    public UnitResult<Error> CompleteCurrentStep(string? resultData = null)
    {
        if (Status != ProcessingStatus.PROCESSING)
            return Error.Validation("processing.invalid.status",
                $"Cannot complete step when status is {Status}");

        ProcessingStep? currentStep = CurrentStep;
        if (currentStep is null)
            return Error.Validation("processing.no.active.step",
                "No active step to complete.");

        UnitResult<Error> completeResult = currentStep.Complete(resultData);
        if (completeResult.IsFailure)
            return completeResult.Error;

        RecalculateProgress();

        return UnitResult.Success<Error>();
    }

    public bool CanRetry() => RetryCount < MaxRetries && !IsCriticalError;

    public UnitResult<Error> Reset()
    {
        if (Status != ProcessingStatus.FAILED)
            return Error.Validation("processing.invalid.status",
                "Can only reset from FAILED status.");

        Status = ProcessingStatus.PROCESSING;
        ProgressPercentage = 0;
        CompletedAt = null;
        ErrorMessage = null;
        IsCriticalError = false;

        foreach (var step in _steps)
        {
            step.Reset();
        }
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ScheduleRetry(DateTime nextRetryAt)
    {
        if (Status != ProcessingStatus.FAILED)
            return Error.Validation("processing.invalid.status",
                "Can only schedule retry steps when status is FAILED.");

        if (IsCriticalError)
            return Error.Validation("processing.retry.critical",
                "Cannot retry critical failure.");

        if (RetryCount >= MaxRetries)
            return Error.Validation("processing.retry.exhausted", "Max retries exceeded.");

        RetryCount++;
        NextRetryAt = nextRetryAt;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> FailCurrentStep(string errorMessage)
    {
        if (Status != ProcessingStatus.PROCESSING)
            return Error.Validation("processing.invalid.status",
                $"Cannot fail step when status is  {Status}");

        var currentStep = CurrentStep;
        if (currentStep is null)
            return Error.Validation("processing.no.active.step",
                $"No active step to fail.");
        
        var failResult = currentStep.Fail(errorMessage);
        if (failResult.IsFailure)
            return failResult.Error;

        return Fail(errorMessage);
    }
    
    internal UnitResult<Error> Fail(string errorMessage, bool isCritical = false)
    {
        if (Status != ProcessingStatus.PROCESSING)
            return Error.Validation("step.invalid.status",$"Can only fail from PROCESSING. Current: {Status}");

        if (string.IsNullOrWhiteSpace(errorMessage))
            return Error.Validation("step.error.required", "Error message is required.");
        
        Status = ProcessingStatus.FAILED;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        IsCriticalError = isCritical;
        
        return  UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete()
    {
        if (Status != ProcessingStatus.PROCESSING)
            return Error.Validation("processing.invalid.status",
                $"Can only complete from PROCESSING status, current: {Status}");
        
        bool allStepsCompleted = _steps.All(s => s.Status == StepStatus.COMPLETED);
        if (!allStepsCompleted)
            return Error.Validation("processing.incomplete.steps",
                $"Cannot complete processing when not all steps are completed.");

        Status = ProcessingStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
        ProgressPercentage = 100;
        
        return UnitResult.Success<Error>();
    }
}

public enum ProcessingStatus
{
    PROCESSING,
    COMPLETED,
    FAILED
}
