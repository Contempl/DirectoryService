namespace DirectoryService.Infrastructure.Options;

public sealed record AuthServiceOptions
{
    public const string SectionName = nameof(AuthServiceOptions);

    public string BaseAddress { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; }
}
