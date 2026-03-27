using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Shared.Kernel;

namespace AuthService.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsActive { get; private set; } = true;
    
    public ICollection<RefreshToken> RefreshTokens  { get; private set; } = new List<RefreshToken>();

    private ApplicationUser() { }

    private ApplicationUser(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<ApplicationUser, Error> Create(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName) || firstName.Length > 100)
            return GeneralErrors.ValueIsInvalid(nameof(firstName));

        if (string.IsNullOrEmpty(lastName) || lastName.Length > 100)
            return GeneralErrors.ValueIsInvalid(nameof(lastName));

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName
        };

        return user;
    }
}