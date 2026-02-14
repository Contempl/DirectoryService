namespace DirectoryService.Application.Options;

public record CachingOptions
{
    public const string SectionName = "CachingOptions";
    
    public string RedisConnectionString { get; set; } = string.Empty;

    public int Expiration { get; set; } = 5;
    public int LocalCacheExpiration { get; set; } = 5;
}