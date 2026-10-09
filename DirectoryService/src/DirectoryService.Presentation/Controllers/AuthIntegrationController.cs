using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Auth;
using DirectoryService.Contracts.Auth;
using DirectoryService.Application.Pagination;
using DirectoryService.Presentation.Response;
using Framework.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/auth-integration")]
public sealed class AuthIntegrationController(
    IQueryHandler<
        GetCurrentUserReportQuery,
        Result<CurrentUserReportDto, Error>> currentUserReportHandler,
    IQueryHandler<
        GetAuthUsersQuery,
        Result<PagedResult<AuthUserSummary>, Error>> authUsersHandler)
    : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<EndpointResult<CurrentUserReportDto>>
        GetCurrentUserReport(CancellationToken cancellationToken)
    {
        return await currentUserReportHandler.HandleAsync(
            new GetCurrentUserReportQuery(),
            cancellationToken);
    }

    [HttpGet("users")]
    [Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
    public async Task<EndpointResult<PagedResult<AuthUserSummary>>>
        GetAuthUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        return await authUsersHandler.HandleAsync(
            new GetAuthUsersQuery(page, pageSize),
            cancellationToken);
    }
}
