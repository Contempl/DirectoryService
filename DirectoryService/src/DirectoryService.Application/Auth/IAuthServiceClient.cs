using CSharpFunctionalExtensions;
using DirectoryService.Application.Pagination;
using Shared.Kernel;

namespace DirectoryService.Application.Auth;

public interface IAuthServiceClient
{
    Task<Result<AuthUserProfile, Error>> GetCurrentUserAsync(
        CancellationToken cancellationToken);

    Task<Result<PagedResult<AuthUserSummary>, Error>> GetUsersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
