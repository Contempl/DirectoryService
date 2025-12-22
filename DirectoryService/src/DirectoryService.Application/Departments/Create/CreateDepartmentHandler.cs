using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Create;

public class CreateDepartmentHandler(
    IDepartmentRepository _departmentRepository,
    ILocationRepository _locationRepository,
    IValidator<CreateDepartmentRequest> _validator,
    ILogger<CreateDepartmentHandler> _logger) : ICommandHandler<Guid, CreateDepartmentRequest>
{
    public async Task<Result<Guid, Errors>> HandleAsync(CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation Failed");

            return validationResult.ToErrors();
        }
        
        Department? parent = null;

        if (request.ParentId.HasValue)
        {
            var availableParent = await _departmentRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            parent = availableParent.Value;
        }
        

        var locationResult = await _locationRepository.CheckIfLocationsExistAsync(request.LocationIds, cancellationToken);

        if (locationResult is false)
        {
            _logger.LogWarning("One of locations does not exist");
            
            return GeneralErrors.ValueIsInvalid("Invalid location provided").ToErrors();
        }


        var nameResult = Name.Create(request.Name);
        
        if (nameResult.IsFailure)
            return nameResult.Error.ToErrors();
        
        var name = nameResult.Value;

        var identifierResult = Identifier.Create(request.Identifier);
        
        if (identifierResult.IsFailure)
            return identifierResult.Error.ToErrors();
        
        var identifier = identifierResult.Value;

        var depId = Guid.NewGuid();
        var departmentLocations = request.LocationIds.Select(lId =>  DepartmentLocation.Create(depId, lId)).ToList();

        var department = Department.Create(name, identifier, request.ParentId, parent, departmentLocations);
        
        if (department.IsFailure)
            return department.Error.ToErrors();

        var departmentId = await _departmentRepository.AddAsync(department.Value, cancellationToken);

        _logger.LogInformation($"Created department with id: {departmentId}");

        return departmentId;
    }
}