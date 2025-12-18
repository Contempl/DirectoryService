using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Create;

public record CreateLocationRequest(CreateLocationDto CreateLocationDto) : ICommand;
