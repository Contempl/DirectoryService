using Microsoft.AspNetCore.Authorization;

namespace AuthService.Core.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}