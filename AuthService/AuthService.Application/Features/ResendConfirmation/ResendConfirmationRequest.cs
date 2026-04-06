using Core.Abstractions;

namespace AuthService.Application.Features.ResendConfirmation;

public record ResendConfirmationRequest(string Email) : ICommand;