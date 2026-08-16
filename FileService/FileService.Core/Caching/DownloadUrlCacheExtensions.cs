using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Kernel;

namespace FileService.Core.Caching;

public static class DownloadUrlCacheExtensions
{
    private static readonly HybridCacheEntryOptions EntryOptions = new()
    {
        Expiration = FileCacheKeys.DownloadUrlExpiration,
        LocalCacheExpiration = FileCacheKeys.DownloadUrlLocalExpiration
    };

    public static async Task<Result<string, Error>> GetDownloadUrlAsync(
        this HybridCache cache,
        Guid mediaAssetId,
        Func<CancellationToken, Task<Result<string, Error>>> factory,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = await cache.GetOrCreateAsync(
                FileCacheKeys.DownloadUrl(mediaAssetId),
                async token =>
                {
                    var result = await factory(token);
                    if (result.IsFailure)
                        throw new DownloadUrlGenerationException(result.Error);

                    return result.Value;
                },
                EntryOptions,
                cancellationToken: cancellationToken);

            return url;
        }
        catch (DownloadUrlGenerationException exception)
        {
            return exception.Error;
        }
    }

    private sealed class DownloadUrlGenerationException(Error error) : Exception(error.Message)
    {
        public Error Error { get; } = error;
    }
}
