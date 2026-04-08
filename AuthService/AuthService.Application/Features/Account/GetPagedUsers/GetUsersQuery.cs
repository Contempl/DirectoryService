using Core.Abstractions;

namespace AuthService.Application.Features.Account.GetPagedUsers;

public record GetUsersQuery(int Page, int PageSize) : IQuery;