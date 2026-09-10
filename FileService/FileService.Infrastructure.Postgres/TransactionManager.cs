using CSharpFunctionalExtensions;
using FileService.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Kernel;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class TransactionManager : ITransactionManager
{
    private readonly IDbContextOutbox<FileServiceDbContext> _outbox;
    private readonly ILogger<TransactionManager> _logger;
    private IDbContextTransaction? _currentTransaction;

    public TransactionManager(ILogger<TransactionManager> logger, IDbContextOutbox<FileServiceDbContext> outbox)
    {
        _logger = logger;
        _outbox = outbox;
    }

    public async Task<UnitResult<Error>> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return Error.Failure("database.transaction.active", "A transaction is already active");

        try
        {
            _currentTransaction = await _outbox.DbContext.Database.BeginTransactionAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to begin transaction");
            return Error.Failure("database.transaction.begin", "Failed to begin transaction");
        }
    }

    // FS-14: EF-изменения и envelope фиксируются вместе, отправка начинается только после commit.
    public async Task<UnitResult<Error>> CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            return Error.Failure("database.transaction.missing", "No active transaction to commit");

        try
        {
            await _outbox.DbContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            await _outbox.FlushOutgoingMessagesAsync();
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogError(exception, "Concurrency conflict while committing transaction");
            await RollbackAsync(CancellationToken.None);
            return Error.Conflict("database.concurrency", "A concurrency conflict occurred");
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception, "Operation cancelled while committing transaction");
            await RollbackAsync(CancellationToken.None);
            return Error.Failure("database.transaction.cancelled", "Transaction commit was cancelled");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to commit transaction");
            await RollbackAsync(CancellationToken.None);
            return Error.Failure("database.transaction.commit", "Failed to commit transaction");
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction is not null)
                await _outbox.DbContext.SaveChangesAsync(cancellationToken);
            else
                await _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogError(exception, "Concurrency conflict while saving changes");
            return Error.Conflict("database.concurrency", "A concurrency conflict occurred");
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception, "Operation cancelled while saving changes");
            return Error.Failure("database.operation.cancelled", "Save operation was cancelled");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save changes");
            return Error.Failure("database", "Failed to save changes");
        }
    }

    private async Task RollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction is not null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to rollback transaction");
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}
