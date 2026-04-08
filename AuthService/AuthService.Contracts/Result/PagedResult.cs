namespace AuthService.Contracts.Result;

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }  
    public long TotalCount { get; set; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    
    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}