using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Kernel;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetsRepository : IMediaAssetsRepository
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<MediaAssetsRepository> _logger;

    public MediaAssetsRepository(FileServiceDbContext dbContext, ILogger<MediaAssetsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public UnitResult<Error> Add(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _dbContext.MediaAssets.Add(mediaAsset);
        return UnitResult.Success<Error>();
    }

    public async Task<Result<MediaAsset, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (asset is null)
            return GeneralErrors.NotFound(id);

        return asset;
    }

    public async Task<Result<VideoAsset, Error>> GetVideoBy(
        Expression<Func<VideoAsset, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var videoAsset = await _dbContext.MediaAssets
            .OfType<VideoAsset>()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (videoAsset is null)
            return GeneralErrors.NotFound();

        return videoAsset;
    }

    public async Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MediaAssets
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitResult<Error>> RemoveAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.MediaAssets.Remove(mediaAsset);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove mediaAsset. {ex}", ex);
            return GeneralErrors.Failure();
        }
        return UnitResult.Success<Error>();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save changes of mediaAsset. {ex}", ex);
        }
    }
}
