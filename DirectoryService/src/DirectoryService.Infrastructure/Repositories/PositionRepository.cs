using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PositionRepository> _logger
        ;

    public PositionRepository(ApplicationDbContext dbContext, ILogger<PositionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> CreatePositionAsync(Position position, CancellationToken cancellationToken)
    {
         await _dbContext.AddAsync(position, cancellationToken);

         try
         {
             await _dbContext.SaveChangesAsync(cancellationToken);

             return position.Id;
         }
         catch (Exception ex)
         {
             return GeneralErrors.ValueIsInvalid("positions").ToErrors();
         }
    }

    public async Task<Result<Position, Error>> GetPositionById(Guid id, CancellationToken cancellationToken)
    {
        var position = await _dbContext.Positions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (position is null)
        {
            _logger.LogInformation("failed to fetch position.");
            return GeneralErrors.NotFound(name: nameof(Position));
        }

        return position;
    }
}
