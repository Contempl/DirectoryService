namespace AuthService.Domain.Constants;

public static class Permissions
{
    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";
    public const string ModerationView = "moderation.view";
    public const string StaffDashboardView = "staff.dashboard.view";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            UsersView,
            UsersManage,
            ModerationView,
            StaffDashboardView
        };
}