using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Update;

public class UpdateLocationHandler : ICommandHandler<UpdateLocationRequest>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<UpdateLocationRequest> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationHandler> _logger;
    

    public UpdateLocationHandler(
        ILocationRepository locationRepository, 
        IValidator<UpdateLocationRequest> validator, 
        ITransactionManager transactionManager, 
        IDepartmentRepository departmentRepository,
        ILogger<UpdateLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
        _departmentRepository = departmentRepository;
    }
    

    public async Task<UnitResult<Errors>> Handle(UpdateLocationRequest request, CancellationToken cancellationToken)
    {
            
        // Проверить — существует ли подразделение с таким departmentId и оно активно
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation of location failed.");
            return validationResult.ToErrors();
        }

        var departmentId = request.LocationDto.departmentId;

        var existingDepartment = await _departmentRepository.GetByIdWithLocationsAsync(departmentId, cancellationToken);
        if (existingDepartment.IsFailure)
        {
            _logger.LogError("department with id {0} could not be found", departmentId);
            return existingDepartment.Error.ToErrors();
        }

        if (existingDepartment.Value.IsActive is false)
        {
            _logger.LogError("department with id {0} is not active", departmentId);
            return GeneralErrors.ValueIsInvalid("department is not active").ToErrors();
        }
        var locationIds = request.LocationDto.locationIds.ToList();
        
        //  Проверить — все locationIds существуют и активны, нет дубликатов
        var existingLocationsResult = await _locationRepository.CheckIfLocationsExistAsync(locationIds, cancellationToken);

        if (existingLocationsResult is false)
        {
            _logger.LogError("one or more locations are not active or don't exist");
            return GeneralErrors.ValueIsInvalid("one or more locations doesn't exist or isn't active").ToErrors();
        }
    
        // Обновить — заменить старые привязки к локациям новым списком
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (!transactionResult.IsSuccess)
            return transactionResult.Error.ToErrors();

        using var transactionScope = transactionResult.Value;

        List<DepartmentLocation> departmentLocations = [];
        foreach (var locationId in locationIds)
        {
            var departmentLocation = new DepartmentLocation(
                departmentId, locationId);
            departmentLocations.Add(departmentLocation);
        }

        var updationResult = existingDepartment.Value.UpdateLocations(departmentLocations);
        if (updationResult.IsFailure)
        {
            transactionScope.Rollback();
            return updationResult.Error.ToErrors();
        }
        transactionScope.Commit();
        
        return UnitResult.Success<Errors>();
    }
}