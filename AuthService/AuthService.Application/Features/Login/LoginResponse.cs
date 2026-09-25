namespace AuthService.Application.Features.Login;

public record LoginResponse(string AccessToken, int ExpiresIn);