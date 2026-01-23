using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Update;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateDepartmentRequest> _createDepartmentHandler;
    private readonly ICommandHandler<UpdateLocationRequest> _updateLocationHandler;
    private readonly ICommandHandler<Guid, UpdateDepartmentRequest> _updateDepartmentHandler;

    public DepartmentController(
        ICommandHandler<Guid, CreateDepartmentRequest> createDepartmentHandler, 
        ICommandHandler<UpdateLocationRequest> updateLocationHandler, 
        ICommandHandler<Guid, UpdateDepartmentRequest> updateDepartmentHandler)
    {
        _createDepartmentHandler = createDepartmentHandler;
        _updateLocationHandler = updateLocationHandler;
        _updateDepartmentHandler = updateDepartmentHandler;
    }
    
    [HttpPost("api/departments")]
    public async Task<EndpointResult<Guid>> CreateDepartment(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return await _createDepartmentHandler.HandleAsync(request, cancellationToken);
    }

    [HttpPut("api/departments/{departmentId}/locations")]
    public async Task<ActionResult<Result<Guid, Errors>>> UpdateLocations([FromRoute] Guid departmentId, IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        var request = new UpdateLocationRequest(departmentId, locationIds);
        
        var result = await _updateLocationHandler.Handle(request, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(departmentId);
    }

    [HttpPut("api/departments/{departmentId}/parent")]
    public async Task<EndpointResult<Guid>> MoveDepartment([FromRoute] Guid departmentId, Guid? parentId,
        CancellationToken cancellationToken)
    {  
        var request = new UpdateDepartmentRequest(departmentId, parentId);
        
        return await _updateDepartmentHandler.HandleAsync(request, cancellationToken);
    }
}