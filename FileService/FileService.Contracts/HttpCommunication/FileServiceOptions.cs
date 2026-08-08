namespace FileService.Contracts.HttpCommunication;

public record FileServiceOptions
{
    public string Url { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;

    public int RetryCount { get; init; } = 2;

    public int RetryDelayMilliseconds { get; init; } = 200;

    public int CircuitSamplingSeconds { get; init; } = 30;

    public double CircuitFailureRatio { get; init; } = 0.5;

    public int CircuitMinimumThroughput { get; init; } = 10;

    public int CircuitBreakSeconds { get; init; } = 15;
}