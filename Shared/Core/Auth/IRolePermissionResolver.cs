namespace Core.Auth;

public interface IRolePermissionResolver
{
    IReadOnlySet<string> Resolve(IEnumerable<string> roles);
}