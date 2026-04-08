using AuthService.Contracts.Dto;
using AuthService.Contracts.Result;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IUserRepository
{
    Task<Result<ApplicationUser, Error>> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}