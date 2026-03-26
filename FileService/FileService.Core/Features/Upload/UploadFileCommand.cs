using Core.Abstractions;

namespace FileService.Core.Features.Upload;

public record UploadFileCommand(UploadFileRequest Request) : ICommand;