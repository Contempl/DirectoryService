namespace FileService.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class FileServiceTestCollection : ICollectionFixture<FileServiceTestWebFactory>
{
    public const string Name = "FileService integration";
}
