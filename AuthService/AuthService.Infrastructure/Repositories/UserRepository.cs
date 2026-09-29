using AuthService.Application;
using AuthService.Contracts.Dto;
using AuthService.Contracts.Result;
using AuthService.Core.Common;
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
    
    public async Task<PagedResult<UserDto>> GetUsersAsync(
        int pageNumber, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users
            .Where(u => u.IsActive)
            .AsQueryable();
        
        var userDtoQuery = query.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,

            Roles = (
            from userRole in _dbContext.UserRoles
            join role in _dbContext.Roles
            on userRole.RoleId equals role.Id
            where userRole.UserId == u.Id
            select role.Name!
            ).ToList()
        });
        
        return await userDtoQuery.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
    }
}
