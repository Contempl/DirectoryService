using Core.Abstractions;

namespace AuthService.Application.Features.Login;

public record LoginRequest(string Email, string Password) : ICommand;