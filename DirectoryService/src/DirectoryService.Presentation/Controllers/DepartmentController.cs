using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Create;
using DirectoryService.Application.Departments.Commands.Delete;
using DirectoryService.Application.Departments.Commands.Update;
using DirectoryService.Application.Departments.Queries.ExpandedDepartments;
using DirectoryService.Application.Departments.Queries.GetChildrenDepartments;
using DirectoryService.Application.Departments.Queries.GetDepartmentDescendants;
using DirectoryService.Application.Departments.Queries.GetListOfDepartments;
using DirectoryService.Application.Locations.UpdateForDepartment;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Presentation.Response;
using Framework.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
    
namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly ICommandHandler<Guid, CreateDepartmentRequest> _createDepartmentHandler;
    private readonly ICommandHandler<UpdateLocationsRequest> _updateLocationHandler;
    private readonly IQueryHandler<bool, PagedResult<DepartmentDto>> _getTopDepartmentsHandler;
    private readonly ICommandHandler<Guid, UpdateDepartmentRequest> _updateDepartmentHandler;
    private readonly ICommandHandler<Guid, DeleteDepartmentRequest> _deleteDepartmentHandler;
    private readonly IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>> _getExpandedDepartmentsHandler;
    private readonly IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>> _getChildrenHandler;
    private readonly IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentShortDto>> _getDepartmentsShortHandler;
    private readonly IQueryHandler<GetDepartmentDescendantIdsQuery, Result<List<Guid>, Errors>> _getDescendantIdsHandler;

    public DepartmentController(
        ICommandHandler<Guid, CreateDepartmentRequest> createDepartmentHandler, 
        ICommandHandler<UpdateLocationsRequest> updateLocationHandler, 
        ICommandHandler<Guid, UpdateDepartmentRequest> updateDepartmentHandler, 
        IQueryHandler<bool, PagedResult<DepartmentDto>> getTopDepartmentsHandler,
        IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>> getExpandedDepartmentsHandler, 
        IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>> getChildrenHandler, 
        IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentShortDto>> getDepartmentsShortHandler,
        IQueryHandler<GetDepartmentDescendantIdsQuery, Result<List<Guid>, Errors>> getDescendantIdsHandler,
        ICommandHandler<Guid, DeleteDepartmentRequest> deleteDepartmentHandler)
    {
        _createDepartmentHandler = createDepartmentHandler;
        _updateLocationHandler = updateLocationHandler;
        _updateDepartmentHandler = updateDepartmentHandler;
        _getTopDepartmentsHandler = getTopDepartmentsHandler;
        _getExpandedDepartmentsHandler = getExpandedDepartmentsHandler;
        _getChildrenHandler = getChildrenHandler;
        _deleteDepartmentHandler = deleteDepartmentHandler;
        _getDepartmentsShortHandler = getDepartmentsShortHandler;
        _getDescendantIdsHandler = getDescendantIdsHandler;
    }
    
    [HttpPost("api/departments")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> CreateDepartment(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return await _createDepartmentHandler.HandleAsync(request, cancellationToken);
    }

    [HttpPut("api/departments/{departmentId}/locations")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<ActionResult<Result<Guid, Errors>>> UpdateLocations([FromRoute] Guid departmentId, IEnumerable<Guid> locationIds, CancellationToken cancellationToken)
    {
        var request = new UpdateLocationsRequest(departmentId, locationIds);
        
        var result = await _updateLocationHandler.Handle(request, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(departmentId);
    }

    [HttpPut("api/departments/{departmentId}/parent")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<EndpointResult<Guid>> MoveDepartment([FromRoute] Guid departmentId, Guid? parentId,
        CancellationToken cancellationToken)
    {  
        var request = new UpdateDepartmentRequest(departmentId, parentId);
        
        return await _updateDepartmentHandler.HandleAsync(request, cancellationToken);
    }

    [HttpGet("api/departments/top-positions")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetTopPositions(bool sortByDescending, CancellationToken cancellationToken)
    {
        var result = await _getTopDepartmentsHandler.HandleAsync(sortByDescending, cancellationToken);

        return Ok(result);
    }

    [HttpGet("api/departments/roots")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<List<DepartmentsWithChildrenDto>>> GetExpandedDepartments(
        [FromQuery] ExtendedDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getExpandedDepartmentsHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("api/departments/tree")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<List<DepartmentsWithChildrenDto>>> GetDepartmentTree(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new ExtendedDepartmentsQuery(page, size, Prefetch: 0);
        var result = await _getExpandedDepartmentsHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("api/departments/{parentId}/children")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<List<DepartmentsWithChildrenDto>>> Children(
        [FromRoute] Guid parentId,
        [FromQuery] GetChildrenRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetChildrenQuery(parentId, request);
        var result = await _getChildrenHandler.HandleAsync(query, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("api/departments/{departmentId}/descendant-ids")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<EndpointResult<List<Guid>>> GetDescendantIds(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        return await _getDescendantIdsHandler.HandleAsync(
            new GetDepartmentDescendantIdsQuery(departmentId),
            cancellationToken);
    }

    [HttpDelete("api/departments/{departmentId}")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_MANAGE}")]
    public async Task<ActionResult<Guid>> DeleteDepartment(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var request = new DeleteDepartmentRequest(departmentId);
        
        var result = await _deleteDepartmentHandler.HandleAsync(request, cancellationToken);

        return Ok(result);
    }

    [HttpGet("api/departments")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<ActionResult<DepartmentShortDto>> GetDepartments(
        [FromQuery] GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getDepartmentsShortHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }
}
