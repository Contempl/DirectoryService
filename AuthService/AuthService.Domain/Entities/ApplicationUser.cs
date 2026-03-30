using System.Security.Cryptography;
using System.Text;
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

    public static Result<ApplicationUser, Error> Create(string firstName, string lastName, string email)
    {
        if (string.IsNullOrEmpty(firstName) || firstName.Length > 100)
            return GeneralErrors.ValueIsInvalid(nameof(firstName));

        if (string.IsNullOrEmpty(lastName) || lastName.Length > 100)
            return GeneralErrors.ValueIsInvalid(nameof(lastName));

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
        };

        return user;
    }

    public static Result<ApplicationUser, Error> SeedAdmin(string firstName, string lastName, string email, string username)
    {
        if (string.IsNullOrEmpty(email))
            return GeneralErrors.ValueIsInvalid(nameof(email));
        
        var userCreationResult = ApplicationUser.Create(firstName, lastName, email);
        
        if (userCreationResult.IsFailure)
            return GeneralErrors.ValueIsInvalid(nameof(ApplicationUser));

        var admin = userCreationResult.Value;
        
        admin.Email = email;
        admin.UserName = username;
        admin.EmailConfirmed = true;

        return admin;
    }
}