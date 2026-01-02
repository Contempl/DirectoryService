using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Update;

public record UpdateLocationRequest(UpdateLocationDto LocationDto) : ICommand;