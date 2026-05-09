using Core.Abstractions;

namespace AuthService.Application.Features.SendPassResetLink;

public record SendResetPasswordRequest(string Email) : ICommand;