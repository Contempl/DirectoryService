using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }
    
    [HttpPost("api/departments")]
    public async Task<EndpointResult<Guid>> CreateDepartment(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return await _departmentService.CreateDepartmentAsync(request, cancellationToken);
    }
}