namespace DirectoryService.Infrastructure.Auth;

internal sealed record AuthServiceEnvelope<T>
{
    public T? Result { get; init; }
    public bool IsError { get; init; }
}

internal sealed record AuthServiceProfileResponse
{
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlySet<string> Permissions { get; init; } =
        new HashSet<string>();

    public bool EmailConfirmed { get; init; }
    public DateTime CreatedAt { get; init; }
}

internal sealed record AuthServiceUserResponse
{
    public Guid Id { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

internal sealed record AuthServicePagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public long TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}