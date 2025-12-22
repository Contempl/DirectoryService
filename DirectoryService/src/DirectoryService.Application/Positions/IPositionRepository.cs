using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<Guid, Errors>> CreatePositionAsync(Position position, CancellationToken cancellationToken);
}