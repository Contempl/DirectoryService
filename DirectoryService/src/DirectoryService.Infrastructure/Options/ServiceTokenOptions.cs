namespace DirectoryService.Infrastructure.Options;

public sealed record ServiceTokenOptions
{
    public const string SectionName = nameof(ServiceTokenOptions);

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public int RefreshBeforeExpirationSeconds { get; init; }
}
