using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Restore;

public record RestoreLocationRequest(Guid LocationId) : ICommand;
