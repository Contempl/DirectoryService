namespace FileService.Core;

public interface IS3Provider
{
    Task<string> GenerateDownloadUrlAsync(Stream stream, string bucketName, string key);
}