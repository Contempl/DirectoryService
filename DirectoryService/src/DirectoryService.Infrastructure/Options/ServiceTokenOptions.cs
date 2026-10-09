namespace DirectoryService.Infrastructure.Options;

public sealed record ServiceTokenOptions
{
    public const string SectionName = nameof(ServiceTokenOptions);

    public Guid SubjectId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public int LifetimeMinutes { get; init; }
    public int RefreshBeforeExpirationSeconds { get; init; }
}
