using Microsoft.AspNetCore.Http;

namespace FileService.Core.Features.Upload;

public record UploadFileRequest(IFormFile FormFile, string AssetType, string Context, Guid ContextId);