using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PositionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
}