using Core.Abstractions;

namespace AuthService.Application.Features.ResetPassword;

public record ResetPasswordRequest(Guid UserId, string Token, string NewPassword) : ICommand;