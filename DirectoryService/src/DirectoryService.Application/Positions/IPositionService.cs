using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionService
{
    Task<Result<Guid, Errors>> HandleAsync(CreatePositionRequest request, CancellationToken cancellationToken);
}