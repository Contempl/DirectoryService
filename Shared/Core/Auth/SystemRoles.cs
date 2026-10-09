namespace Core.Auth;

public static class SystemRoles
{
    public const string User = "User";
    public const string Employee = "Employee";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";
    public const string ServiceAccount = "ServiceAccount";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            User,
            Employee,
            Moderator,
            Admin,
            ServiceAccount
        };
}
