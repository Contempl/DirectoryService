using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler(
    ILocationRepository _locationRepository,
    IValidator<CreateLocationRequest> _validator,
    ILogger<CreateLocationHandler> _logger)
    : ICommandHandler<Guid, CreateLocationRequest>
{

    public async Task<Result<Guid, Errors>> HandleAsync(CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation Failed");

            return validationResult.ToErrors();
        }

        var locationName = Name.Create(request.CreateLocationDto.Name).Value;

        var locationAddress = Address.Create(request.CreateLocationDto.City, 
            request.CreateLocationDto.Street, request.CreateLocationDto.House,
            request.CreateLocationDto.Apartment).Value;

        var timezone = Timezone.Create(request.CreateLocationDto.Timezone).Value;
        
        var location = Location.Create(locationName, locationAddress, timezone).Value;

        var locationResult = await _locationRepository.CreateAsync(location, cancellationToken);

        _logger.LogInformation($"Created location with id: {locationResult}");

        return locationResult;
    }
}