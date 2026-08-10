using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryBaseTests : IClassFixture<DirectoryServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected FakeFileCommunicationService FileService { get; }
    protected IServiceProvider Services { get; set; }

    protected DirectoryBaseTests(DirectoryServiceTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
        FileService = factory.FileService;
    }

    protected async Task<T> ExecuteInDb<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        return await action(sut);
    }

    protected async Task ExecuteInDb(Func<ApplicationDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await action(sut);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        FileService.Reset();
        await _resetDatabase();
    }
}
