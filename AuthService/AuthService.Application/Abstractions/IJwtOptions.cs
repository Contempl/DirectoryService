namespace AuthService.Application.Abstractions;

public interface IJwtOptions
{ 
    int AccessTokenLifetimeMinutes { get; }
}