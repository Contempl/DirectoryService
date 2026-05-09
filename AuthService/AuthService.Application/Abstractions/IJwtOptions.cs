namespace AuthService.Application.Abstractions;

public interface IJwtOptions
{ 
    public string Issuer { get;  }

    public string Audience { get;  }

    public string Secret { get; }

    public int AccessTokenLifetimeMinutes { get; }

    public int RefreshTokenLifetime { get;  }
}