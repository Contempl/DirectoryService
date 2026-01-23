using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<DepartmentLocation> DepartmentLocationsRead { get; }

    IQueryable<DepartmentPosition> DepartmentPositionsRead { get; }

    IQueryable<Location> LocationsRead { get; }

    IQueryable<Position> PositionsRead { get; }
}