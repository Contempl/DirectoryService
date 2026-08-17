namespace FileService.Infrastructure;

public sealed class CacheOptions
{
    public const string SectionName = "CacheOptions";

    public string RedisConnectionString { get; init; } = string.Empty;
    public int ExpirationTimeInMinutes { get; init; } = 35;
    public int LocalCacheExpiration { get; init; } = 5;
}