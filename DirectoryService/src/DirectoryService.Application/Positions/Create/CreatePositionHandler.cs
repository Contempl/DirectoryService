using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionHandler(
    IPositionRepository _positionRepository,
    IDepartmentRepository _departmentRepository,
    IValidator<CreatePositionRequest> _validator,
    ILogger<CreatePositionHandler> _logger) : ICommandHandler<Guid, CreatePositionRequest>
{
    public async Task<Result<Guid, Errors>> HandleAsync(CreatePositionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation Failed");

            return validationResult.ToErrors();
        }

        var nameResult = Name.Create(request.Name);
        
        if (nameResult.IsFailure) 
            return nameResult.Error.ToErrors();
        
        var name = nameResult.Value;

        var departmentsExistResult = await _departmentRepository.CheckIfDepartmentsExistAsync(request.DepartmentIds, cancellationToken);
        
        if (departmentsExistResult is false)
        {
            _logger.LogWarning("One of departments does not exist or was deleted");
            
            return GeneralErrors.ValueIsInvalid("Invalid department provided").ToErrors();
        }

        var departmentPositions = request.DepartmentIds.Select(departmentId => DepartmentPosition.Create(departmentId, Guid.NewGuid())).ToList();
        
        var position = Position.Create(name, request.Description, departmentPositions).Value;
        
        var positions = await _positionRepository.CreatePositionAsync(position, cancellationToken);
        
        if (positions.IsFailure)
            return positions.Error;
        
        return positions.Value;
    }
}