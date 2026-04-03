using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Update;

public record UpdatePositionCommand(Guid PositionId, UpdatePositionRequest Request) : ICommand;