namespace DirectoryService.Domain.Shared;


public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is invalid", name);
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $"по id {id}";
        return Error.NotFound("value.not.found", $"{name ?? "value"} not found {forId}");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? string.Empty;
        return Error.Validation("length.is.invalid", $"Field {label} is required", name);
    }

    public static Error AlreadyExist()
    {
        return Error.Conflict("record.already.exist", $"record already exists");
    }

    public static Error Failure()
    {
        return Error.Failure("server.failure", "Server failure");
    }
}