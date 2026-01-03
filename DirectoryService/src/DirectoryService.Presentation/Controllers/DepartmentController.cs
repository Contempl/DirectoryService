using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly UpdateLocationHandler _updateLocationHandler;

    public DepartmentController(IDepartmentService departmentService, UpdateLocationHandler updateLocationHandler)
    {
        _departmentService = departmentService;
        _updateLocationHandler = updateLocationHandler;
    }
    
    [HttpPost("api/departments")]
    public async Task<EndpointResult<Guid>> CreateDepartment(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return await _departmentService.CreateDepartmentAsync(request, cancellationToken);
    }
    
    [HttpPut("api/departments/{departmentId}/locations")]
    public async Task<Result<Guid, Errors>> UpdateLocations([FromRoute] Guid departmentId, IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        var request = new UpdateLocationRequest(departmentId, locationIds);
        
        var result = await _updateLocationHandler.Handle(request, cancellationToken);
        if (result.IsFailure)
            return result.Error;

        return departmentId;
    }
}