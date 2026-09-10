using FileService.Core.Messaging;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Messaging;

public sealed class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IDbContextOutbox<FileServiceDbContext> _dbContextOutbox;

    public IntegrationEventPublisher(IDbContextOutbox<FileServiceDbContext> dbContextOutbox)
    {
        _dbContextOutbox = dbContextOutbox;
    }

    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await _dbContextOutbox.PublishAsync(integrationEvent);
    }
}