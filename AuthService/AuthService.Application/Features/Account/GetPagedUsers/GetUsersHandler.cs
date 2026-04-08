using AuthService.Contracts.Dto;
using AuthService.Contracts.Result;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.GetPagedUsers;

public class GetUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersHandler> _logger;
    
    public GetUsersHandler(IUserRepository userRepository, ILogger<GetUsersHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<UserDto>, Error>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (request.PageSize < 1 || request.PageSize > 100)
        {
            _logger.LogInformation("PageSize must be between 1 and 100");
            return GeneralErrors.ValueIsInvalid(nameof(request));
        }
        
        var users = await _userRepository.GetUsersAsync(request.Page, request.PageSize, cancellationToken);

        return users;
    }

}