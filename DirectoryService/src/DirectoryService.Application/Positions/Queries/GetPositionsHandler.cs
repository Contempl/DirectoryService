using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Pagination;
using DirectoryService.Contracts.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Positions.Queries;

public class GetPositionsHandler : IQueryHandler<GetPositionsQuery, PagedResult<PositionDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetPositionsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<PagedResult<PositionDto>> HandleAsync(GetPositionsQuery query, CancellationToken cancellationToken)
    {
        var positionsQuery = _readDbContext.PositionsRead;
        
        if (!string.IsNullOrWhiteSpace(query.Search))
            positionsQuery = positionsQuery.Where(l =>
                EF.Functions.Like(l.Name.Value.ToLower(), $"%{query.Search.ToLower()}%"));
        
        positionsQuery = positionsQuery.Where(l => l.IsActive == query.IsActive);

        if (query.DepartmentIds is { Length: > 0 })
        {
            positionsQuery = positionsQuery.Where(loc => 
                loc.DepartmentPositions.Any(dl => query.DepartmentIds.Contains(dl.DepartmentId)));
        }

        var totalCount = await positionsQuery.LongCountAsync(cancellationToken);

        positionsQuery = positionsQuery
            .OrderBy(l => l.UpdatedAt)
            .ThenBy(l => l.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var positions = await positionsQuery
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name.Value,
                Description = p.Description ?? string.Empty,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt =  p.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PositionDto>
        {
            Items = positions,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
            TotalCount = totalCount
        };
    }
}
