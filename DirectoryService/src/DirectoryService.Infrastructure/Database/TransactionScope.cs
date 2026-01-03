using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Database;

public class TransactionScope : ITransactionScope, IDisposable
{
    private readonly IDbTransaction _dbTransaction;
    private readonly ILogger<ITransactionScope> _logger;

    public TransactionScope(IDbTransaction dbTransaction, ILogger<ITransactionScope> logger)
    {
        _dbTransaction = dbTransaction;
        _logger = logger;
    }
    
    public UnitResult<Error> Commit()
    {
        try
        {
            _dbTransaction.Commit();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to begin transaction");
            return Error.Failure(
                "transaction.commit.failed", 
                "Failed to commit transaction");
        }
    }

    public void Dispose()
    {
        _dbTransaction.Dispose();
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _dbTransaction.Rollback();
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to rollback transaction");
            return Error.Failure(
                "transaction.rollback.failed", 
                "Failed to rollback transaction");
        }
    }
}