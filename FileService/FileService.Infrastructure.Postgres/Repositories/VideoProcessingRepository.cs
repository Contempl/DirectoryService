using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;

namespace FileService.Infrastructure.Postgres.Repositories;

public class VideoProcessingRepository : IVideoProcessingRepository
{
    private readonly FileServiceDbContext _dbContext;

    public VideoProcessingRepository(FileServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<VideoProcessing, Error>> GetByAsync(
        Expression<Func<VideoProcessing, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var videoProcessing = await _dbContext.VideoProcesses
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (videoProcessing == null)
            return GeneralErrors.NotFound();

        return videoProcessing;
    }

    public Result<Guid,Error> Add(VideoProcessing videoProcessing)
    {
        _dbContext.VideoProcesses.Add(videoProcessing);
        return videoProcessing.Id;
    }
}