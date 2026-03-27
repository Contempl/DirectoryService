using Core.Validation;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using FluentValidation;
using Shared.Kernel;

namespace FileService.Core.Features.Upload;

public class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.Failure());
        
        RuleFor(f => f.Request.FormFile)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UploadFileRequest.FormFile)));
        
        RuleFor(f => f.Request.FormFile.ContentType).MustBeValueObject(ContentType.Create);
        
        RuleFor(f => f.Request.FormFile.Length).GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UploadFileRequest.FormFile)));
        
        RuleFor(f => f.Request.AssetType)
            .Must(type => Enum.TryParse<AssetType>(type, true, out _))
            .WithError(GeneralErrors.Failure());
        
        RuleFor(f => f.Request)
            .MustBeValueObject(r => MediaOwner.Create(r.Context, r.ContextId));
    }
}