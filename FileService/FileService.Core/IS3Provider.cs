using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result<string, Error>> GenerateDownloadUrlAsync(Stream stream, string bucketName, string key);

    Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> DownloadFileAsync(StorageKey key, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> StartMultipartUploadAsync(string bucketName, string key, string contentType, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateChunkUploadUrl(StorageKey key, string uploadId, int partNumber);

    Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(StorageKey key, string uploadId, int totalChunks, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> CompleteMultipartUploadAsync(StorageKey key, string uploadId, List<PartETagDto> partETags, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MultipartUploadEntry>, Error>> ListMultipartUploadAsync(string bucketName, CancellationToken cancellationToken = default);
}
