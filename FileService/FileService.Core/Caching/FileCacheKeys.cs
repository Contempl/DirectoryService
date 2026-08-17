namespace FileService.Core.Caching;

public static class FileCacheKeys
{
    private const string DownloadUrlPrefix = "file:download-url:";

    public static string DownloadUrl(Guid fileId) =>
        $"{DownloadUrlPrefix}{fileId:D}";
}
