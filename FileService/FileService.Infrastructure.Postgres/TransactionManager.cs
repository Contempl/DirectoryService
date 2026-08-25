using System.Data;
using System.Transactions;
using CSharpFunctionalExtensions;
using FileService.Core;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Infrastructure.Postgres;

public class TransactionManager : ITransactionManager
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionManager(FileServiceDbContext dbContext,
        ILogger<TransactionScope> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        return transaction.GetDbTransaction();
    }

    public async Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
            return Error.Failure("database", "Failed to save changes");
        }
    }
}