using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Domain.ValueObjects;

public record MediaData
{
    public FileName FileName { get; }

    public ContentType ContentType { get; }

    public long Size { get; set; }

    public int ExpectedChunksCount { get; set; }
    private MediaData() { }

    private MediaData(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
    {
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        ExpectedChunksCount = expectedChunksCount;
    }

    public static Result<MediaData, Error> Create(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
    {
        if (size <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(size));

        if (expectedChunksCount <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(expectedChunksCount));
        
        return new MediaData(fileName, contentType, size, expectedChunksCount);
    }
}