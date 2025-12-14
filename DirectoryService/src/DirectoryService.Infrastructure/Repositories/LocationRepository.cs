using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LocationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(location, cancellationToken);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid("location").ToErrors();
        }

        return location.Id;
    }
}