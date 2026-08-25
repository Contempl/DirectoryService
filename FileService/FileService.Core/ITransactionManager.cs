using System.Data;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.Core;

public interface ITransactionManager
{
    Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}