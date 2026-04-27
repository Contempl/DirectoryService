namespace Framework.Options;

public record JwtOptions : IJwtOptions
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; }

    public int RefreshTokenLifetime { get; init; }
}

public interface IJwtOptions
{ 
    public string Issuer { get;  }

    public string Audience { get;  }

    public string Secret { get; }

    public int AccessTokenLifetimeMinutes { get; }

    public int RefreshTokenLifetime { get;  }
}