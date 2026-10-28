using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Delete;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class DeleteLocationTests : DirectoryBaseTests
{
    public DeleteLocationTests(DirectoryServiceHandlerTestFixture factory) : base(factory)
    {

    }

    [Fact]
    public async Task DeleteLocation_WhenLocationExists_MarksItInactive()
    {
        // Arrange
        using var source  = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        Guid locationId = await CreateLocationAsync(ct);

        // Act
        await using (var scope = Services.CreateAsyncScope())
        {
            var deleteHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, DeleteLocationRequest>>();

            var deleteResult = await deleteHandler.HandleAsync(
                new DeleteLocationRequest(locationId), ct);

            Assert.True(deleteResult.IsSuccess);
        }

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations.SingleAsync(l => l.Id == locationId, ct);

            Assert.False(location.IsActive);
        });
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var source  = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        Guid locationId = Guid.NewGuid();

        // Act & Assert
        await using (var scope = Services.CreateAsyncScope())
        {
            var deleteHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, DeleteLocationRequest>>();

            var result = await deleteHandler.HandleAsync(
                new DeleteLocationRequest(locationId), ct);

            Assert.True(result.IsFailure);
            Assert.Contains(result.Error, error => error.Code == "value.not.found");
        }

    }

    [Fact]
    public async Task DeleteLocation_WhenLocationAlreadyDeleted_ReturnsNotFound()
    {
        // Arrange
        using var source  = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        Guid locationId = await CreateLocationAsync(ct);

        await using (var scope = Services.CreateAsyncScope())
        {
            var deleteHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, DeleteLocationRequest>>();

            var deleteResult = await deleteHandler.HandleAsync(
                new DeleteLocationRequest(locationId), ct);

            Assert.True(deleteResult.IsSuccess);
        }

        // Act & Assert
        await using (var scope = Services.CreateAsyncScope())
        {
            var deleteHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, DeleteLocationRequest>>();

            var deleteResult = await deleteHandler.HandleAsync(
                new DeleteLocationRequest(locationId), ct);

            Assert.True(deleteResult.IsFailure);
            Assert.Contains(deleteResult.Error, error => error.Code == "value.not.found");
        }
    }

    private async Task<Guid> CreateLocationAsync(CancellationToken ct)
    {
        var request = new CreateLocationDto("Museum", "Ryazan", "Pushkina", "8", null, "UTC");
        await using (var scope = Services.CreateAsyncScope())
        {
            var createHandler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, CreateLocationRequest>>();

            var createResult = await createHandler.HandleAsync(
                new CreateLocationRequest(request), ct);

            Assert.True(createResult.IsSuccess);
            return createResult.Value;
        }
    }
}
