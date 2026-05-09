using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.ValueObjects;

public record MediaOwner
{
    private static readonly HashSet<string> AllowedContexts =
    [
        "lesson",
        "user",
        "course",
        "department",
    ];
    
    public string Context { get; }

    public Guid EntityId { get; }

    private MediaOwner() { }
    
    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }

    public static Result<MediaOwner, Error> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length >= 50)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        var normalizedContext = context.Trim().ToLowerInvariant();
        if (!AllowedContexts.Contains(normalizedContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));
        
        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));
        
        var contextToLowered = context.ToLowerInvariant();
        
        return new MediaOwner(contextToLowered, entityId);
    }

    public static Result<MediaOwner, Error> ForLesson(Guid lessonId) => Create("lesson", lessonId);
    public static Result<MediaOwner, Error> ForCourse(Guid courseId) => Create("course", courseId);
    public static Result<MediaOwner, Error> ForUser(Guid userId) => Create("user", userId);
    public static Result<MediaOwner, Error> ForDepartment(Guid departmentId) => Create("department", departmentId);
}