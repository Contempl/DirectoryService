using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    UnitResult<Error> Rollback();
}