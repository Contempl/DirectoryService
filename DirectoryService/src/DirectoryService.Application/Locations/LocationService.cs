using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class LocationService(ILocationRepository locationRepository, IValidator<CreateLocationDto> locationValidator) : ILocationService
{
    

    public async Task<Guid> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await locationValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        var locationName = Name.Create(request.Name).Value;
        
        var locationAddress = Address.Create(request.Address.City, request.Address.Street, request.Address.House, request.Address.Apartment).Value;
        
        var location = Location.Create(locationName, locationAddress, request.Timezone).Value;
        
        var locationId = await locationRepository.AddAsync(location, cancellationToken);
        
        return locationId;
    }
}