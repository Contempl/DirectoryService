namespace AuthService.Core.Options;

public record BackgroundServiceOptions
{
    public int RemoveRevokedTokenIntervalHours { get; init; }

    public int ThresholdTokensDays { get; init; }
}