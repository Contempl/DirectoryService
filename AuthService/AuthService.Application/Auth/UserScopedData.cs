namespace AuthService.Application.Auth;

public class UserScopedData
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public IReadOnlySet<string> Permissions { get; private set; } = new HashSet<string>();
    
    public bool IsAuthenticated { get; private set; }

    public void Authenticate(Guid userId, string email, string name, 
        IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        UserId = userId;
        Email = email;
        Name = name;
        Roles = roles.ToList();
        Permissions = permissions.ToHashSet();
        IsAuthenticated = true;
    }
}