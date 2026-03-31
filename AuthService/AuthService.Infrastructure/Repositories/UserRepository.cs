using AuthService.Application;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;

namespace AuthService.Core.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ApplicationUser, Error>> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return GeneralErrors.NotFound(name: nameof(ApplicationUser));

        return user;
    }
}