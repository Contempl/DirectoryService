using Core.Abstractions;

namespace AuthService.Application.Features.Register;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName) : ICommand;