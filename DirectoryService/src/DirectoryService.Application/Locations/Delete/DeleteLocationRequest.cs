using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Delete;

public record DeleteLocationRequest(Guid LocationId) : ICommand;