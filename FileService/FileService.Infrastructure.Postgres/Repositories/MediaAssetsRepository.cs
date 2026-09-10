using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetsRepository : IMediaAssetsRepository
{
    private readonly IDbContextOutbox<FileServiceDbContext> _dbContextOutbox;
    private readonly ILogger<MediaAssetsRepository> _logger;

    public MediaAssetsRepository(ILogger<MediaAssetsRepository> logger,
        IDbContextOutbox<FileServiceDbContext> dbContextOutbox)
    {
        _logger = logger;
        _dbContextOutbox = dbContextOutbox;
    }

    public UnitResult<Error> Add(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _dbContextOutbox.DbContext.MediaAssets.Add(mediaAsset);
        return UnitResult.Success<Error>();
    }

    public async Task<Result<MediaAsset, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _dbContextOutbox.DbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (asset is null)
            return GeneralErrors.NotFound(id);

        return asset;
    }

    public async Task<Result<VideoAsset, Error>> GetVideoBy(
        Expression<Func<VideoAsset, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var videoAsset = await _dbContextOutbox.DbContext.MediaAssets
            .OfType<VideoAsset>()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (videoAsset is null)
            return GeneralErrors.NotFound();

        return videoAsset;
    }

    public async Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContextOutbox.DbContext.MediaAssets
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitResult<Error>> RemoveAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContextOutbox.DbContext.MediaAssets.Remove(mediaAsset);
            await _dbContextOutbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove mediaAsset.");
            return GeneralErrors.Failure();
        }
        return UnitResult.Success<Error>();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContextOutbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Failed to save changes of mediaAsset.");
            throw;
        }
    }
}
