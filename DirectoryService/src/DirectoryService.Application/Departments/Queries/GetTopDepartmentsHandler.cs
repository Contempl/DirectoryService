using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Departments;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopDepartmentsHandler : IQueryHandler<bool, PagedResult<DepartmentDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetTopDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<PagedResult<DepartmentDto>> HandleAsync(bool sortByDescending, CancellationToken cancellationToken)
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
    }
}