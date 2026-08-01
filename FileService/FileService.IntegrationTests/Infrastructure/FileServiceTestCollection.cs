namespace FileService.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class FileServiceTestCollection : ICollectionFixture<FileServiceTestWebFactory>
{
    public const string Name = "FileService integration";
}
