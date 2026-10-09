using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Auth;

public sealed record GetAuthUsersQuery(
    int Page = 1,
    int PageSize = 20)
    : IQuery;
