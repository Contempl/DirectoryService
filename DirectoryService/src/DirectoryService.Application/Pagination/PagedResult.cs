namespace DirectoryService.Application.Pagination;

public record PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; }
    
    public long TotalCount { get; init; }
}