namespace FileService.Core.Features.StreamVideoProcessingStatus;

public sealed record VideoProcessingStatusStreamOptions
{
    public const string SectionName = "VideoProcessingStatusStream";

    public int PollingIntervalSeconds { get; init; } = 1;

    public int HeartbeatIntervalSeconds { get; init; } = 15;
}
