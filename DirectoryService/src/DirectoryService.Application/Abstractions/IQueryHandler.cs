namespace DirectoryService.Application.Abstractions;

public interface IQuery;
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}