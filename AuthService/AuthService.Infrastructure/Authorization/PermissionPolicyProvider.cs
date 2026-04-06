using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthService.Core.Authorization;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)  : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Permission:";
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);
    
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        _fallback.GetFallbackPolicyAsync();
}