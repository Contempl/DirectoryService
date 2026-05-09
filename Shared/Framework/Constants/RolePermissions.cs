namespace Framework.Constants;

public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> _mapping = new(StringComparer.OrdinalIgnoreCase)
    {
        [Roles.Participant] = [Permissions.CONTENT_VIEW],
        [Roles.Author] =
        [
            Permissions.CONTENT_VIEW,
            Permissions.CONTENT_MANAGE,
            Permissions.FILES_MANAGE,
        ],
        [Roles.Moderator] =
        [
            Permissions.CONTENT_VIEW,
            Permissions.CONTENT_MANAGE,
            Permissions.FILES_MANAGE,
            Permissions.USERS_MANAGE,
        ],
        [Roles.Admin] = [.. Permissions.AllPermissions]
    };

    public static HashSet<string> GetPermissions(IEnumerable<string> groups)
    {
        HashSet<string> permissions = [];

        foreach (string group in groups)
        {
            if (_mapping.TryGetValue(group, out HashSet<string>? rolePerms))
            {
                permissions.UnionWith(rolePerms);
            }
        }

        return permissions;
    }
}
