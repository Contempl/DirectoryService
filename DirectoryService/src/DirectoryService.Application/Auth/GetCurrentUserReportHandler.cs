using Core.Auth;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Auth;
using Shared.Kernel;

namespace DirectoryService.Application.Auth;

public sealed class GetCurrentUserReportHandler(
    IAuthServiceClient authServiceClient,
    UserScopedData userScopedData)
    : IQueryHandler<
        GetCurrentUserReportQuery,
        Result<CurrentUserReportDto, Error>>
{
    public async Task<Result<CurrentUserReportDto, Error>> HandleAsync(
        GetCurrentUserReportQuery query,
        CancellationToken cancellationToken)
    {
        var profileResult = await authServiceClient
            .GetCurrentUserAsync(cancellationToken);

        if (profileResult.IsFailure)
        {
            return profileResult.Error;
        }

        var profile = profileResult.Value;

        return new CurrentUserReportDto(
            userScopedData.UserId,
            userScopedData.Email,
            userScopedData.Name,
            userScopedData.Roles,
            userScopedData.Permissions,
            profile.AuthPermissions,
            profile.EmailConfirmed,
            profile.CreatedAt);
    }
}
