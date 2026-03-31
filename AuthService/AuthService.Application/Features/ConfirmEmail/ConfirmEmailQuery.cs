using Core.Abstractions;

namespace AuthService.Application.Features.ConfirmEmail;

public record ConfirmEmailQuery(Guid UserId, string Token) : IQuery;