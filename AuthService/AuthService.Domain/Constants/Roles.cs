namespace AuthService.Domain.Constants;

public static class Roles
{
    public const string Participant = "participant";
    public const string Author      = "author";
    public const string Moderator   = "moderator";
    public const string Admin       = "admin";


    public static readonly IReadOnlyList<string> AllRoles =
    [
        Participant,
        Author,
        Moderator,
        Admin
    ];
}