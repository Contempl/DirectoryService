namespace FileService.Core.Caching;

public static class FileCacheKeys
{
    private const string DownloadUrlPrefix = "file:download-url:";

    public static readonly TimeSpan DownloadUrlExpiration = TimeSpan.FromMinutes(35);

    public static readonly TimeSpan DownloadUrlLocalExpiration = TimeSpan.FromMinutes(5);

    public static string DownloadUrl(Guid fileId) =>
        $"{DownloadUrlPrefix}{fileId:D}";
}
