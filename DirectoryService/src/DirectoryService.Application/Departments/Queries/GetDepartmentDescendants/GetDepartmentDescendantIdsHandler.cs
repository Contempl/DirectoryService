using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentDescendants;

public class GetDepartmentDescendantIdsHandler
    : IQueryHandler<GetDepartmentDescendantIdsQuery, Result<List<Guid>, Errors>>
{
    private readonly IDepartmentRepository _departmentRepository;

    public GetDepartmentDescendantIdsHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<List<Guid>, Errors>> HandleAsync(
        GetDepartmentDescendantIdsQuery query,
        CancellationToken cancellationToken)
    {
        var departmentResult = await _departmentRepository.GetByIdAsNoTrackingAsync(
            query.DepartmentId,
            cancellationToken);

        if (departmentResult.IsFailure || !departmentResult.Value.IsActive)
            return DepartmentMoveErrors.DepartmentNotFound(query.DepartmentId).ToErrors();

        return await _departmentRepository.GetDescendantIdsAsync(
            query.DepartmentId,
            cancellationToken);
    }
}
