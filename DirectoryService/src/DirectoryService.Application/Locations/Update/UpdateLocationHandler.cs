using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations.UpdateForDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation; 
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Update;

public class UpdateLocationHandler : ICommandHandler<Location ,UpdateLocationRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateLocationRequest> _validator;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationHandler(ILocationRepository locationRepository,
        IValidator<UpdateLocationRequest> validator,
        ILogger<UpdateLocationsHandler> logger, 
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager; 
        _logger = logger;
    }

    public async Task<Result<Location, Errors>> HandleAsync(UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("validation failed for update location request.");
            return validationResult.ToErrors();
        }
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        var existingLocationResult =  await _locationRepository
            .GetLocationByIdAsync(request.LocationId, cancellationToken);

        if (existingLocationResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError($"location with id {request.LocationId} not found.");
            
            return existingLocationResult.Error.ToErrors();
        }
        
        var existingLocation = existingLocationResult.Value;

        var nameCreationResult = Name.Create(request.LocationDto.Name);
        if (nameCreationResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to create name VO for location.");
            
            return nameCreationResult.Error.ToErrors();
        }

        var name = nameCreationResult.Value;

        var addressCreationResult = Address.Create(
            request.LocationDto.City,
            request.LocationDto.Street,
            request.LocationDto.House,
            request.LocationDto.Apartment);

        if (addressCreationResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to create address VO for location.");
            
            return addressCreationResult.Error.ToErrors();
        }

        var address = addressCreationResult.Value;

        var timezoneCreationResult = Timezone.Create(request.LocationDto.Timezone);

        if (timezoneCreationResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to create timezone VO for location.");
            
            return timezoneCreationResult.Error.ToErrors();
        }

        var timezone = timezoneCreationResult.Value;

        var updateResult = existingLocation.Update(name, address, timezone);

        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to update location inside entity.");
            
            updateResult.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("failed to save changes via transaction.");
            
            return saveChangesResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Failed to commit  transaction.");
            
            return commitResult.Error.ToErrors();
        }

        return existingLocation;

    }
}