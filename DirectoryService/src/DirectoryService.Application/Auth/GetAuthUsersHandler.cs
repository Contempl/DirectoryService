using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Pagination;
using Shared.Kernel;

namespace DirectoryService.Application.Auth;

public sealed class GetAuthUsersHandler(
    IAuthServiceClient authServiceClient)
    : IQueryHandler<
        GetAuthUsersQuery,
        Result<PagedResult<AuthUserSummary>, Error>>
{
    public Task<Result<PagedResult<AuthUserSummary>, Error>> HandleAsync(
        GetAuthUsersQuery query,
        CancellationToken cancellationToken)
    {
        return authServiceClient.GetUsersAsync(
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}
