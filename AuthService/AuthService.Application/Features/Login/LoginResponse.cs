namespace AuthService.Application.Features.Login;

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn);