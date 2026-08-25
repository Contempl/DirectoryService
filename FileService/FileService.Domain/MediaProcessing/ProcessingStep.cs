using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.MediaProcessing;

public class ProcessingStep
{
    public Guid Id { get; private set; }


    public int Order { get; private set; }

    public int Weight { get; private set; }

    public StepType Type { get; private set; }

    public StepStatus Status { get; private set; }

    public string? ResultData { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    private ProcessingStep() { }

    public ProcessingStep(StepType stepType, int order, int weight)
    {
        Id = Guid.NewGuid();
        Type =  stepType;
        Order = order;
        Weight = weight;
        Status = StepStatus.PENDING;
    }
    
    internal UnitResult<Error> Start()
    {
        if (Status != StepStatus.PENDING)
            return Error.Validation("step.invalid.status",
                $"Can only start step from pending status. Current: {Status}");

        Status = StepStatus.PROCESSING;
        StartedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> Complete(string? resultData = null)
    {
        if (Status != StepStatus.PROCESSING)
            return Error.Validation("step.invalid.status",
                $"Can only complete step from processing status. Current: {Status}");
        
        Status = StepStatus.COMPLETED;
        ResultData = resultData;
        CompletedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }
    
    internal UnitResult<Error> Fail(string errorMessage)
    {
        if (Status != StepStatus.PROCESSING)
            return Error.Validation("step.invalid.status",$"Can only fail from PROCESSING. Current: {Status}");

        if (string.IsNullOrWhiteSpace(errorMessage))
            return Error.Validation("step.error.required", "Error message is required.");
        
        Status = StepStatus.FAILED;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        
        return  UnitResult.Success<Error>();
    }

    internal void Reset()
    {
        Status = StepStatus.PENDING;
        ResultData = null;
        ErrorMessage = null;
        StartedAt = null;
        CompletedAt = null;
    }
    
}

public enum StepStatus
{
    PENDING,
    PROCESSING,
    COMPLETED,
    FAILED
}

public enum StepType
{
    INITIALIZE,
    EXTRACT_METADATA,
    GENERATE_HLS,
    UPLOAD_HLS,
    GENERATE_PREVIEW,
    CLEANUP
}
