using AuthService.Domain.Constants;
using Core.Auth;

namespace AuthService.Domain.Authorization;

public sealed class AuthRolePermissionResolver : IRolePermissionResolver
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Mapping =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemRoles.User] = new HashSet<string>(),
            [SystemRoles.Employee] = new HashSet<string>
            {
                Permissions.StaffDashboardView
            },
            [SystemRoles.Moderator] = new HashSet<string>
            {
                Permissions.ModerationView
            },
            [SystemRoles.Admin] = new HashSet<string>(Permissions.All)
        };

    public IReadOnlySet<string> Resolve(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (Mapping.TryGetValue(role, out var rolePermissions))
                permissions.UnionWith(rolePermissions);
        }

        return permissions;
    }
}
