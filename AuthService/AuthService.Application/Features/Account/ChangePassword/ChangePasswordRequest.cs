using Core.Abstractions;

namespace AuthService.Application.Features.Account.ChangePassword;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword) : ICommand;