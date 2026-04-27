namespace Framework.Constants;

public static class Permissions
{
    public const string CONTENT_VIEW  = "content.view";
    public const string CONTENT_MANAGE = "content.manage";
    public const string FILES_MANAGE  = "files.manage";
    public const string USERS_MANAGE  = "users.manage";
    public const string SYSTEM_ADMIN  = "system.admin";

    public static readonly IReadOnlyList<string> AllPermissions =
    [
        CONTENT_VIEW,
        CONTENT_MANAGE,
        FILES_MANAGE,
        USERS_MANAGE,
        SYSTEM_ADMIN
    ];
}