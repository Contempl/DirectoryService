using FileService.Domain.MediaProcessing;

namespace FileService.VideoProcessingService.UnitTests;

public class VideoProcessingTests
{
    [Fact]
    public void ProcessNextStep_WhenProcessIsNew_StartsInitializeStep()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());

        // Act
        var processNextStep = videoProcessing.ProcessNextStep();

        // Assert
        Assert.True(processNextStep.IsSuccess);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(StepStatus.PROCESSING, processNextStep.Value!.Status);
        Assert.Equal(StepType.INITIALIZE, processNextStep.Value!.Type);
    }

    [Fact]
    public void ProcessNextStep_WhenStepAlreadyProcessing_ReturnsSameStep()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        var processNextStep = videoProcessing.ProcessNextStep();

        // Act
        var processNextStepTwice = videoProcessing.ProcessNextStep();

        // Assert
        Assert.True(processNextStep.IsSuccess);
        Assert.True(processNextStepTwice.IsSuccess);
        Assert.NotNull(processNextStepTwice.Value);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(processNextStep.Value.Id, processNextStepTwice.Value.Id);
    }

    [Fact]
    public void CompleteCurrentStep_WhenNoStepIsActive_ReturnsError()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());

        // Act
        var completeCurrentStep = videoProcessing.CompleteCurrentStep();

        // Assert
        Assert.True(completeCurrentStep.IsFailure);
        Assert.Equal("processing.no.active.step", completeCurrentStep.Error.Code);
    }

    [Fact]
    public void CompleteCurrentStep_WhenStepIsActive_MarksStepCompleted()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        var processNextStep = videoProcessing.ProcessNextStep();

        // Act
        var completeCurrentStep = videoProcessing.CompleteCurrentStep();

        // Assert
        Assert.True(completeCurrentStep.IsSuccess);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(StepStatus.COMPLETED, processNextStep.Value.Status);
    }

    [Fact]
    public void ProcessNextStep_AfterCompletingCurrentStep_StartsNextStep()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        var processInitializingStep = videoProcessing.ProcessNextStep();
        var completeInitializing = videoProcessing.CompleteCurrentStep();

        // Act
        var processNextStep = videoProcessing.ProcessNextStep();

        // Assert
        Assert.True(processNextStep.IsSuccess);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(StepStatus.PROCESSING, processNextStep.Value.Status);
        Assert.Equal(StepType.EXTRACT_METADATA, processNextStep.Value.Type);
    }

    [Fact]
    public void CompleteCurrentStep_WhenMetadataStepCompleted_UpdatesProgressToTenPercent()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        var processInitializingStep = videoProcessing.ProcessNextStep();
        var completeInitializing = videoProcessing.CompleteCurrentStep();

        // Act
        var processNextStep = videoProcessing.ProcessNextStep();
        var completeExtractingMetadata = videoProcessing.CompleteCurrentStep();

        // Assert
        Assert.True(processNextStep.IsSuccess);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(StepType.EXTRACT_METADATA, processNextStep.Value.Type);
        Assert.True(completeExtractingMetadata.IsSuccess);
        Assert.Equal(10, videoProcessing.ProgressPercentage);
    }

    [Fact]
    public void FailCurrentStep_WhenStepIsActive_MarksProcessAsFailed()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        _ = videoProcessing.ProcessNextStep();

        // Act
        var failCurrentStep = videoProcessing.FailCurrentStep("test error");

        // Assert
        Assert.True(failCurrentStep.IsSuccess);
        Assert.Equal(ProcessingStatus.FAILED, videoProcessing.Status);
        Assert.Equal("test error", videoProcessing.ErrorMessage);
    }

    [Fact]
    public void Reset_WhenProcessFailed_RestoresInitialState()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        var processNextStep = videoProcessing.ProcessNextStep();
        var failCurrentStep = videoProcessing.FailCurrentStep("test error");

        // Act
        var resetResult = videoProcessing.Reset();

        // Assert
        Assert.True(failCurrentStep.IsSuccess);
        Assert.True(resetResult.IsSuccess);
        Assert.Equal(ProcessingStatus.PROCESSING, videoProcessing.Status);
        Assert.NotNull(processNextStep.Value);
        Assert.Equal(StepStatus.PENDING, processNextStep.Value.Status);
        Assert.Null(videoProcessing.ErrorMessage);
    }

    [Fact]
    public void ScheduleRetry_WhenProcessFailed_SetsNextRetryAndIncrementsCount()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        _ = videoProcessing.ProcessNextStep();
        _ = videoProcessing.FailCurrentStep("test error");
        var nextRetryAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var result = videoProcessing.ScheduleRetry(nextRetryAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, videoProcessing.RetryCount);
        Assert.Equal(nextRetryAt, videoProcessing.NextRetryAt);
    }

    [Fact]
    public void ScheduleRetry_WhenMaxRetriesReached_ReturnsError()
    {
        // Arrange
        var videoProcessing = new Domain.MediaProcessing.VideoProcessing(Guid.NewGuid());
        _ = videoProcessing.ProcessNextStep();
        _ = videoProcessing.FailCurrentStep("test error");
        var nextRetryAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var scheduleRetry1 = videoProcessing.ScheduleRetry(nextRetryAt);
        var scheduleRetry2 = videoProcessing.ScheduleRetry(nextRetryAt);
        var scheduleRetry3 = videoProcessing.ScheduleRetry(nextRetryAt);
        var scheduleRetry4 = videoProcessing.ScheduleRetry(nextRetryAt);

        // Assert

        Assert.True(scheduleRetry1.IsSuccess);
        Assert.True(scheduleRetry2.IsSuccess);
        Assert.True(scheduleRetry3.IsSuccess);
        Assert.True(scheduleRetry4.IsFailure);
        Assert.Equal("processing.retry.exhausted", scheduleRetry4.Error.Code);
        Assert.Equal(videoProcessing.MaxRetries, videoProcessing.RetryCount);
    }
}
