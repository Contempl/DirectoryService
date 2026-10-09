namespace DirectoryService.Infrastructure.Options;

public sealed record AuthUsersSynchronizationOptions
{
    public const string SectionName =
        nameof(AuthUsersSynchronizationOptions);

    public bool Enabled { get; init; }
    public int IntervalMinutes { get; init; }
    public int PageSize { get; init; }
}
