using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IUserRepository
{
    Task<Result<ApplicationUser, Error>> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}