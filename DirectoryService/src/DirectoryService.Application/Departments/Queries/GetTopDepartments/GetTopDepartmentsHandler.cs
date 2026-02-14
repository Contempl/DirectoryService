using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartments;

public class GetTopDepartmentsHandler : IQueryHandler<bool, PagedResult<DepartmentDto>>
{
    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;

    public GetTopDepartmentsHandler(IReadDbContext readDbContext, HybridCache cache)
    {
        _readDbContext = readDbContext;
        _cache = cache;
    }

    public async Task<PagedResult<DepartmentDto>> HandleAsync(bool sortByDescending,
        CancellationToken cancellationToken)
    {
        var cacheKey = string.Concat(
            $"{Constants.DEPARTMENT_CACHE_KEY}",
            $"{Constants.TOP_FIVE_DEPARTMENTS_TAG}");


        var departments = await _cache.GetOrCreateAsync<PagedResult<DepartmentDto>>(
            cacheKey,
            factory: async _ =>
            {
                var query = _readDbContext.DepartmentsRead
                    .Include(d => d.Positions)
                    .OrderByDescending(d =>
                        _readDbContext.DepartmentPositionsRead
                            .Count(p => p.DepartmentId == d.Id))
                    .Take(5);

                var departments = await query.Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        ParentId = d.ParentId,
                        Name = d.Name.Value,
                        Identifier = d.Identifier.Value,
                        Positions = d.Positions.Select(dp => dp.PositionId).ToArray(),
                        Path = d.Path.Value,
                        Depth = d.Depth,
                        ChildrenCount = d.ChildrenCount,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                    })
                    .ToArrayAsync(cancellationToken: cancellationToken);

                var result = new PagedResult<DepartmentDto>
                {
                    Data = departments,
                    TotalCount = departments.Length,
                };

                return result;
            }, 
            tags: [Constants.DEPARTMENT_CACHE_KEY],
            cancellationToken: cancellationToken);

        return departments;
    }
}