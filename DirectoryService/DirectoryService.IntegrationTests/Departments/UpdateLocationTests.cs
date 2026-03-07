using System;
using System.Threading;
using System.Threading.Tasks;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Update;
using DirectoryService.Application.Locations.UpdateForDepartment;
using DirectoryService.IntegrationTests.Departments.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationTests : DepartmentsBaseTests
{
    public UpdateLocationTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocation_WithValidData_ShouldUpdateLocation()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var locationIds = await CreateLocationsAsync(ct);
        var departmentWithLocations = await CreateDepartmentWithLocationsAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationsRequest(departmentWithLocations.Id, locationIds);

            return sut.Handle(command, ct);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentWithLocations.Id, ct);

            Assert.NotNull(department);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, department.Id);
        });
    }

    [Fact]
    public async Task UpdateLocation_WithOneLocation_ShouldUpdateSuccessfully()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var locationId = await CreateLocation(ct);
        var department = await CreateDepartmentAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationsRequest(department.Id, [locationId]);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.NotNull(department);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateLocation_WithTheSameLocation_ShouldFallWithError()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        
        var department = await CreateDepartmentAsync(ct);
        var locationId = await CreateLocation(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationsRequest(department.Id, [locationId, locationId]);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateLocation_WithInvalidData_ShouldReturnError()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var departmentWithLocations = await CreateDepartmentWithLocationsAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationsRequest(departmentWithLocations.Id, []);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<UpdateLocationsRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateLocationsRequest>>();

        return await action(sut);
    }
}