using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using Path = DirectoryService.Domain.Entities.VO.Path;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Errors>> CreateAsync(Department department, CancellationToken cancellationToken = default);
    
    Task<Result<Department, Errors>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Department, Errors>> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> DeleteLocationsByDepAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> AddDepLocationsRelationsAsync(List<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken = default);
    
    Task<bool> CheckIfDepartmentsExistAsync(List<Guid> departmentIds, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdWithLocationsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result<Department, Errors>> GetByIdWithLock(Guid departmentId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetDescendantIdsAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> MoveDepartment(Guid departmentId, Guid parentId, Path parentPath, Path departmentPath,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> MoveDepartment(Guid departmentId, Path departmentPath,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateTreePathsAsync(Path oldPath, Path newPath,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeactivateOrphanedLocationsAsync(Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeactivateOrphanedPositionsAsync(Guid departmentId,
        CancellationToken cancellationToken = default);
}
