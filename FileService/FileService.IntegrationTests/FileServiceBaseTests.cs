using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests;

public abstract class FileServiceBaseTests : IClassFixture<FileServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected IServiceProvider Services { get; }

    protected FileServiceBaseTests(FileServiceTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    protected async Task<T> ExecuteInDb<T>(Func<FileServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await action(dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();
}
