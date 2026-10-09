using System.Net;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Auth;
using DirectoryService.Application.Pagination;
using Shared.Kernel;

namespace DirectoryService.Infrastructure.Auth;

internal sealed class AuthServiceClient(HttpClient httpClient)
    : IAuthServiceClient
{
    public async Task<Result<AuthUserProfile, Error>> GetCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            "api/auth/me",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound(
                "auth.profile.not.found",
                "AuthService profile was not found");
        }

        var responseError = MapResponseError(response);
        if (responseError is not null)
        {
            return responseError;
        }

        var envelope = await response.Content
            .ReadFromJsonAsync<
                AuthServiceEnvelope<AuthServiceProfileResponse>>(
                cancellationToken);

        if (envelope is null ||
            envelope.IsError ||
            envelope.Result is null)
        {
            return Error.Failure(
                "auth.service.invalid.response",
                "AuthService returned an invalid profile response");
        }

        var profile = envelope.Result;

        return new AuthUserProfile(
            profile.Roles,
            profile.Permissions,
            profile.EmailConfirmed,
            profile.CreatedAt);
    }

    public async Task<Result<PagedResult<AuthUserSummary>, Error>>
        GetUsersAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"api/users?Page={page}&PageSize={pageSize}",
            cancellationToken);

        var responseError = MapResponseError(response);
        if (responseError is not null)
        {
            return responseError;
        }

        var envelope = await response.Content
            .ReadFromJsonAsync<
                AuthServiceEnvelope<
                    AuthServicePagedResponse<AuthServiceUserResponse>>>(
                cancellationToken);

        if (envelope is null ||
            envelope.IsError ||
            envelope.Result is null)
        {
            return Error.Failure(
                "auth.service.invalid.response",
                "AuthService returned an invalid users response");
        }

        var authPage = envelope.Result;

        var users = authPage.Items
            .Select(user => new AuthUserSummary(
                user.Id,
                user.Roles,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsActive))
            .ToArray();

        return new PagedResult<AuthUserSummary>
        {
            Items = users,
            TotalCount = authPage.TotalCount,
            Page = authPage.PageNumber,
            PageSize = authPage.PageSize,
            TotalPages = authPage.TotalPages
        };
    }

    private static Error? MapResponseError(
        HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                Error.Unauthorized(
                    "AuthService rejected the access token"),

            HttpStatusCode.Forbidden =>
                Error.Forbidden(
                    "AuthService denied access to the resource"),

            _ when !response.IsSuccessStatusCode =>
                Error.Failure(
                    "auth.service.request.failed",
                    $"AuthService returned HTTP {(int)response.StatusCode}"),

            _ => null
        };
    }
}
