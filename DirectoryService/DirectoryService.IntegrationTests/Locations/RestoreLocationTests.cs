using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Application.Locations.Restore;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class RestoreLocationTests : DirectoryBaseTests
{
    public RestoreLocationTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RestoreLocation_AfterSoftDelete_ShouldMakeLocationActive()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = source.Token;
        var locationId = await CreateLocation(cancellationToken);

        await ExecuteCommand<DeleteLocationRequest>(
            new DeleteLocationRequest(locationId),
            cancellationToken);

        var restoreResult = await ExecuteCommand<RestoreLocationRequest>(
            new RestoreLocationRequest(locationId),
            cancellationToken);

        Assert.True(restoreResult.IsSuccess);
        Assert.Equal(locationId, restoreResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations.SingleAsync(l => l.Id == locationId, cancellationToken);
            Assert.True(location.IsActive);
        });
    }

    private async Task<Guid> CreateLocation(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<Guid, CreateLocationRequest>>();
        var dto = new CreateLocationDto(
            $"Restorable location {Guid.NewGuid()}",
            "Moscow",
            "Archive street",
            Guid.NewGuid().ToString(),
            null,
            "UTC");

        var result = await handler.HandleAsync(new CreateLocationRequest(dto), cancellationToken);
        return result.Value;
    }

    private async Task<CSharpFunctionalExtensions.Result<Guid, Shared.Kernel.Errors>> ExecuteCommand<TCommand>(
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, TCommand>>();
        return await handler.HandleAsync(command, cancellationToken);
    }
}
