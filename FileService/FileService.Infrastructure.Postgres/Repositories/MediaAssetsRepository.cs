using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
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

    public async Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _dbContext.MediaAssets.Add(mediaAsset);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return mediaAsset.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            return GeneralErrors.Failure();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was cancelled while creating mediaAsset");
            return GeneralErrors.Failure();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating mediaAsset");

            return GeneralErrors.Failure();
        }
    }
}
