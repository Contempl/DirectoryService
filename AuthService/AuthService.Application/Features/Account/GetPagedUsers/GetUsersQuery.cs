using Core.Abstractions;

namespace AuthService.Application.Features.Account.GetPagedUsers;

public record GetUsersQuery(int Page = 1, int PageSize = 10) : IQuery;