using AuthService.Application.Extensions;
using AuthService.Contracts.Dto;
using AuthService.Contracts.Result;
using AuthService.Domain.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.GetPagedUsers;

public class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async Task<EndpointResult<PagedResult<UserDto>>> (
            [FromQuery] int Page,
            [FromQuery] int PageSize,
            [FromServices] GetUsersHandler handler, 
            CancellationToken cancellationToken) =>
        {
            var request = new GetUsersQuery(Page, PageSize);
            return await handler.HandleAsync(request, cancellationToken);
        }).RequirePermissions(Permissions.USERS_MANAGE);
    }
}