using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Departments;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.Queries.GetListOfDepartments;

public class GetDepartmentsHandler : IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentShortDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<PagedResult<DepartmentShortDto>> HandleAsync(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var departmentsQuery = _readDbContext.DepartmentsRead;

        if (query.DepartmentIds is { Length: > 0 })
            departmentsQuery = departmentsQuery.Where(d => query.DepartmentIds.Contains(d.Id));

        if (query.LocationsIds?.Length > 0)
            departmentsQuery = departmentsQuery.Where(d =>
                query.LocationsIds.All(id => d.Locations.Any(l => l.LocationId == id)));

        if (!string.IsNullOrWhiteSpace(query.Search))
            departmentsQuery = departmentsQuery.Where(d =>
                EF.Functions.ILike(d.Name.Value, $"%{query.Search}%"));
                
        if (query.IsActive is not null)
            departmentsQuery = departmentsQuery.Where(d => d.IsActive == query.IsActive);

        if (query.ParentId is not null)
            departmentsQuery = departmentsQuery.Where(d => d.ParentId == query.ParentId);
        
        var totalCount =  await departmentsQuery.LongCountAsync(cancellationToken);

        var isDescending = query.SortDirection?.ToLower() == "desc";
   
        departmentsQuery = (query.SortBy ?? "name").ToLower() switch
        {
            "path" => isDescending 
                ? departmentsQuery.OrderBy(d => d.Path.Value)
                : departmentsQuery.OrderByDescending(d => d.Path.Value),
            
            "created" => isDescending 
                ? departmentsQuery.OrderBy(d => d.CreatedAt)
                : departmentsQuery.OrderByDescending(d => d.CreatedAt),
            
            _ => isDescending
                ? departmentsQuery.OrderBy(d => d.Name.Value)
                : departmentsQuery.OrderByDescending(d => d.Name.Value)
        };

        departmentsQuery = departmentsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var departments = await departmentsQuery
            .Select(d => new DepartmentShortDto 
            {
                Id = d.Id,
                Name = d.Name.Value,
                Identifier = d.Identifier.Value,
                Path = d.Path.Value,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt!.Value 
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentShortDto>
        {
            TotalCount = totalCount,
            Items = departments,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
