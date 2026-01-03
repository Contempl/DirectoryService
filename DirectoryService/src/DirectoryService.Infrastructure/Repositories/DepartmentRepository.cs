using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(ApplicationDbContext dbContext, ILogger<DepartmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> CreateAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(department, cancellationToken);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Created department with id: {department.Id}");
            
            return Result.Success<Guid, Errors>(department.Id);
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid("invalid.department.values").ToErrors();
        }
    }

    public async Task<Result<Department, Errors>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Departments.FindAsync(id, cancellationToken);

        if (result is null)
        {
            return GeneralErrors.NotFound().ToErrors();
        }

        return result;
    }
    
    public async Task<Result<Department, Error>> GetByIdWithLocationsAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.Locations) 
            .Where(d => d.IsActive)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (department == null)
            return GeneralErrors.NotFound();

        return department;
    }
    
    public async Task<bool> CheckIfDepartmentsExistAsync(List<Guid> departmentIds, CancellationToken cancellationToken = default)
    {
        var foundCount = await _dbContext.Departments
            .Where(l => departmentIds.Contains(l.Id) && l.IsActive)
            .CountAsync(cancellationToken);

        return foundCount == departmentIds.Count;
    }
}