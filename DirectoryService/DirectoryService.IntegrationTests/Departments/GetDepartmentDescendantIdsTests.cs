using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Queries.GetDepartmentDescendants;
using DirectoryService.Domain.Shared;
using DirectoryService.IntegrationTests.Departments.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentDescendantIdsTests : DepartmentsBaseTests
{
    public GetDepartmentDescendantIdsTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDescendantIds_ShouldReturnEveryDescendantWithoutSelectedDepartment()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            IQueryHandler<GetDepartmentDescendantIdsQuery, Result<List<Guid>, Errors>>>();

        var result = await handler.HandleAsync(
            new GetDepartmentDescendantIdsQuery(departmentIds[2]),
            ct);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(departmentIds[2], result.Value);
        Assert.Equal(departmentIds.Skip(3), result.Value);
    }
}
