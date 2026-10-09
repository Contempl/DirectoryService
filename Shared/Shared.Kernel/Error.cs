using System.Text.Json.Serialization;

namespace Shared.Kernel;

public record Error
{
    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public string? InvalidField { get; }
    
    [JsonConstructor]
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error NotFound(string? code, string message) =>
        new(code ?? "value.not.found", message, ErrorType.NOT_FOUND);

    public static Error Validation(string? code, string message, string? invalidField = null) =>
        new(code ?? "value.is.invalid", message,  ErrorType.VALIDATION, invalidField);

    public static Error Failure(string? code, string message) =>
        new(code ?? "failure", message,  ErrorType.FAILURE);

    public static Error Conflict(string? code, string message) =>
        new(code ?? "value.is.conflict", message, ErrorType.CONFLICT);

    public static Error Unauthorized(
        string message = "Authentication is required",
        string? code = null) =>
        new(code ?? "authentication.required", message, ErrorType.UNAUTHORIZED);

    public static Error Forbidden(
        string message = "Access is forbidden",
        string? code = null) =>
        new(code ?? "access.forbidden", message, ErrorType.FORBIDDEN);

    public Errors ToErrors() => new([this]);
}

public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
    UNAUTHORIZED,
    FORBIDDEN,
}
