using Core.Abstractions;

namespace AuthService.Application.Features.RefreshToken;

public record RefreshTokenRequest(string RefreshToken) : ICommand;