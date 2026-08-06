using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.BulkActivityUpdate;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class BulkUpdateLocationsActivityTests : DirectoryBaseTests
{
    public BulkUpdateLocationsActivityTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Archive_WithSeveralLocations_ShouldMakeAllLocationsInactive()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = source.Token;
        var locationIds = await CreateLocations(2, cancellationToken);

        var result = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest(locationIds, IsActive: false),
            cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ProcessedCount);
        Assert.Empty(result.Value.Errors);

        await ExecuteInDb(async dbContext =>
        {
            var locations = await dbContext.Locations
                .Where(location => locationIds.Contains(location.Id))
                .ToListAsync(cancellationToken);

            Assert.Equal(2, locations.Count);
            Assert.All(locations, location => Assert.False(location.IsActive));
        });
    }

    [Fact]
    public async Task Restore_WithArchivedLocations_ShouldMakeAllLocationsActive()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = source.Token;
        var locationIds = await CreateLocations(2, cancellationToken);

        var archiveResult = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest(locationIds, IsActive: false),
            cancellationToken);
        Assert.True(archiveResult.IsSuccess);

        var restoreResult = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest(locationIds, IsActive: true),
            cancellationToken);

        Assert.True(restoreResult.IsSuccess);
        Assert.Equal(2, restoreResult.Value.ProcessedCount);
        Assert.Empty(restoreResult.Value.Errors);

        await ExecuteInDb(async dbContext =>
        {
            var locations = await dbContext.Locations
                .Where(location => locationIds.Contains(location.Id))
                .ToListAsync(cancellationToken);

            Assert.All(locations, location => Assert.True(location.IsActive));
        });
    }

    [Fact]
    public async Task Update_WithUnknownId_ShouldProcessExistingLocationAndReportError()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = source.Token;
        var locationId = (await CreateLocations(1, cancellationToken)).Single();
        var unknownId = Guid.NewGuid();

        var result = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest([locationId, unknownId], IsActive: false),
            cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ProcessedCount);
        var error = Assert.Single(result.Value.Errors);
        Assert.Equal(unknownId, error.LocationId);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations.SingleAsync(
                item => item.Id == locationId,
                cancellationToken);
            Assert.False(location.IsActive);
        });
    }

    [Fact]
    public async Task Update_WithDuplicateIds_ShouldProcessLocationOnce()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var cancellationToken = source.Token;
        var locationId = (await CreateLocations(1, cancellationToken)).Single();

        var result = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest([locationId, locationId], IsActive: false),
            cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ProcessedCount);
        Assert.Empty(result.Value.Errors);
    }

    [Fact]
    public async Task Update_WithEmptyIds_ShouldReturnValidationError()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var result = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest([], IsActive: false),
            source.Token);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
        Assert.All(result.Error, error => Assert.Equal(ErrorType.VALIDATION, error.Type));
    }

    [Fact]
    public async Task Update_WithMoreThanOneHundredIds_ShouldReturnValidationError()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var locationIds = Enumerable.Range(0, 101)
            .Select(_ => Guid.NewGuid())
            .ToList();

        var result = await ExecuteBulkCommand(
            new BulkUpdateLocationsActivityRequest(locationIds, IsActive: false),
            source.Token);

        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.VALIDATION, error.Type);
        Assert.Equal("locations.bulk.ids.limit", error.Code);
    }

    private async Task<List<Guid>> CreateLocations(int count, CancellationToken cancellationToken)
    {
        var ids = new List<Guid>(count);

        for (var index = 0; index < count; index++)
        {
            await using var scope = Services.CreateAsyncScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<Guid, CreateLocationRequest>>();
            var uniqueValue = Guid.NewGuid();
            var dto = new CreateLocationDto(
                $"Bulk location {uniqueValue}",
                "Moscow",
                "Bulk street",
                uniqueValue.ToString(),
                null,
                "UTC");

            var result = await handler.HandleAsync(
                new CreateLocationRequest(dto),
                cancellationToken);

            Assert.True(result.IsSuccess);
            ids.Add(result.Value);
        }

        return ids;
    }

    private async Task<Result<BulkUpdateLocationsActivityResult, Errors>> ExecuteBulkCommand(
        BulkUpdateLocationsActivityRequest request,
        CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            ICommandHandler<BulkUpdateLocationsActivityResult, BulkUpdateLocationsActivityRequest>>();

        return await handler.HandleAsync(request, cancellationToken);
    }
}
