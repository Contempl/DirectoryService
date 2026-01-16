using System.Text.Json.Serialization;

namespace DirectoryService.Domain.Shared;

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

    public Errors ToErrors() => new([this]);
}

public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
}