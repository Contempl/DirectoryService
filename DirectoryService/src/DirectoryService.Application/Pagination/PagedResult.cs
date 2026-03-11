namespace DirectoryService.Application.Pagination;

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }  
    public long TotalCount { get; set; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}