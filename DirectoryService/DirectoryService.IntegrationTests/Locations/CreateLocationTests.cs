using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class CreateLocationTests : DirectoryBaseTests
{
    public CreateLocationTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new CreateLocationDto("Museum", "Ryazan", "Pushkina", "8", null, "UTC");
        await using var scope = Services.CreateAsyncScope();

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateLocationRequest(request);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstAsync(d => d.Id == result.Value, CancellationToken.None);

            Assert.NotNull(location);
            Assert.True(location.Id == result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateLocation_WithInvalidName_ShouldReturnError()
    {
        // Arrange
        var request = new CreateLocationDto("", "Moscow", "", "9", null, "UTC");

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateLocationRequest(request);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }
    
    [Fact]
    public async Task CreateLocation_WithInvalidAddress_ShouldReturnError()
    {
        // Arrange
        var request = new CreateLocationDto("Box Office", "", "", "", null, "UTC");

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateLocationRequest(request);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateLocation_WithInvalidTimezone_ShouldFallWithError()
    {
        // Arrange
        var request = new CreateLocationDto("Museum", "Ryazan", "Pushkina", "8", null, "Jupiter");

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateLocationRequest(request);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<Guid, CreateLocationRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, CreateLocationRequest>>();

        return await action(sut);
    }
}