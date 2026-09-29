using Core.Auth;
using Framework.Constants;

namespace DirectoryService.Presentation.Authorization;

public sealed class DirectoryRolePermissionResolver : IRolePermissionResolver
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Mapping =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemRoles.User] = new HashSet<string> { Permissions.CONTENT_VIEW },
            [SystemRoles.Employee] = new HashSet<string> { Permissions.CONTENT_VIEW },
            [SystemRoles.Moderator] = new HashSet<string>
            {
                Permissions.CONTENT_VIEW,
                Permissions.CONTENT_MANAGE
            },
            [SystemRoles.Admin] = new HashSet<string>
            {
                Permissions.CONTENT_VIEW,
                Permissions.CONTENT_MANAGE
            }
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
