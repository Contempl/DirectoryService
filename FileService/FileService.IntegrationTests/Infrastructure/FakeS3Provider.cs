using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.IntegrationTests.Infrastructure;

public class FakeS3Provider : IS3Provider
{
    public Task<Result<string, Error>> GenerateDownloadUrlAsync(Stream stream, string bucketName, string key) =>
        Task.FromResult(Result.Success<string, Error>("https://fake-s3/download"));

    public Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default) =>
        Task.FromResult(UnitResult.Success<Error>());

    public Task<Result<string, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<string, Error>("https://fake-s3/download"));

    public Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<string, Error>(key.Value));

    public Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> urls = keys.Select(_ => "https://fake-s3/download").ToList();
        return Task.FromResult(Result.Success<IReadOnlyList<string>, Error>(urls));
    }

    public Task<Result<string, Error>> StartMultipartUploadAsync(string bucketName, string key, string contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<string, Error>("fake-upload-id"));

    public Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<string, Error>("fake-upload-id"));

    public Task<Result<string, Error>> GenerateChunkUploadUrl(StorageKey key, string uploadId, int partNumber) =>
        Task.FromResult(Result.Success<string, Error>($"https://fake-s3/upload/{partNumber}"));

    public Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(StorageKey key, string uploadId, int totalChunks, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ChunkUploadUrl> urls = Enumerable.Range(1, totalChunks)
            .Select(i => new ChunkUploadUrl { PartNumber = i, UploadUrl = $"https://fake-s3/upload/{i}" })
            .ToList();
        return Task.FromResult(Result.Success<IReadOnlyList<ChunkUploadUrl>, Error>(urls));
    }

    public Task<UnitResult<Error>> CompleteMultipartUploadAsync(StorageKey key, string uploadId, List<PartETagDto> partETags, CancellationToken cancellationToken = default) =>
        Task.FromResult(UnitResult.Success<Error>());

    public Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId, CancellationToken cancellationToken = default) =>
        Task.FromResult(UnitResult.Success<Error>());

    public Task<Result<IReadOnlyList<MultipartUploadEntry>, Error>> ListMultipartUploadAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MultipartUploadEntry> entries = [];
        return Task.FromResult(Result.Success<IReadOnlyList<MultipartUploadEntry>, Error>(entries));
    }
}
