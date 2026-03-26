using Core.Abstractions;

namespace FileService.Core.Features.Delete;

public record DeleteFileRequest(Guid FileId) : ICommand;