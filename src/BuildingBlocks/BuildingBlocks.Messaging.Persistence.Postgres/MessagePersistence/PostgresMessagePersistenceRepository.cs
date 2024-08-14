using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class PostgresMessagePersistenceRepository(
    MessagePersistenceDbContext persistenceDbContext,
    ILogger<PostgresMessagePersistenceRepository> logger
) : IMessagePersistenceRepository
{
    private readonly ILogger<PostgresMessagePersistenceRepository> _logger = logger;

    public async Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        await persistenceDbContext.StoreMessages.AddAsync(storeMessage, cancellationToken);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        persistenceDbContext.StoreMessages.Update(storeMessage);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeStateAsync(
        Guid messageId,
        MessageStatus status,
        CancellationToken cancellationToken = default
    )
    {
        // tacked entity here by EF
        var message = await persistenceDbContext.StoreMessages.FirstOrDefaultAsync(
            x => x.Id == messageId,
            cancellationToken
        );
        if (message is not null)
        {
            message.ChangeState(status);
            await persistenceDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await persistenceDbContext.StoreMessages.AsNoTracking().ToListAsync(cancellationToken)).AsReadOnly();
    }

    public async Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await persistenceDbContext.StoreMessages.Where(predicate).AsNoTracking().ToListAsync(cancellationToken)
        ).AsReadOnly();
    }

    public async Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // tacked entity here by EF
        return await persistenceDbContext.StoreMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> RemoveAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        persistenceDbContext.StoreMessages.Remove(storeMessage);
        var res = await persistenceDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task CleanupMessages()
    {
        if (!await persistenceDbContext.StoreMessages.AnyAsync())
            return;

        persistenceDbContext.StoreMessages.RemoveRange(persistenceDbContext.StoreMessages);

        await persistenceDbContext.SaveChangesAsync();
    }
}
