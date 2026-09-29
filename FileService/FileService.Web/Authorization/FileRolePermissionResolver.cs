using Core.Auth;
using Framework.Constants;

namespace FileService.Authorization;

public sealed class FileRolePermissionResolver : IRolePermissionResolver
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Mapping =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemRoles.User] = new HashSet<string>(),
            [SystemRoles.Employee] = new HashSet<string>(),
            [SystemRoles.Moderator] = new HashSet<string> { Permissions.FILES_MANAGE },
            [SystemRoles.Admin] = new HashSet<string> { Permissions.FILES_MANAGE }
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
