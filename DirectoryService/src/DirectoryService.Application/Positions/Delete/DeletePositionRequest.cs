using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Delete;

public record DeletePositionRequest(Guid PositionId) : ICommand;