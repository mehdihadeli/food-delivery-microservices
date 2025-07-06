using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class PostgresMessagePersistenceRepository(
    MessagePersistenceDbContext persistenceDbContext,
    ILogger<PostgresMessagePersistenceRepository> logger
) : IMessagePersistenceRepository
{
    public async Task AddAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        await persistenceDbContext.PersistMessages.AddAsync(persistMessage, cancellationToken);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        persistenceDbContext.PersistMessages.Update(persistMessage);

        await persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeStateAsync(
        Guid messageId,
        MessageStatus status,
        CancellationToken cancellationToken = default
    )
    {
        // tacked entity here by EF
        var message = await persistenceDbContext.PersistMessages.FirstOrDefaultAsync(
            x => x.MessageId == messageId,
            cancellationToken
        );
        if (message is not null)
        {
            message.ChangeState(status);
            await persistenceDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<PersistMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await persistenceDbContext.PersistMessages.AsNoTracking().ToListAsync(cancellationToken)).AsReadOnly();
    }

    public async Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        Expression<Func<PersistMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await persistenceDbContext.PersistMessages.Where(predicate).AsNoTracking().ToListAsync(cancellationToken)
        ).AsReadOnly();
    }

    public Task<IReadOnlyList<PersistMessage>> GetFilterBySateAsync(
        MessageStatus? status = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PersistMessage>> GetFilterByAsync(
        MessageStatus? status = null,
        MessageDeliveryType? deliveryType = null,
        string? dataType = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<PersistMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // tacked entity here by EF
        return await persistenceDbContext.PersistMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> RemoveAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        persistenceDbContext.PersistMessages.Remove(persistMessage);
        var res = await persistenceDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task CleanupMessages()
    {
        if (!await persistenceDbContext.PersistMessages.AnyAsync())
            return;

        persistenceDbContext.PersistMessages.RemoveRange(persistenceDbContext.PersistMessages);

        await persistenceDbContext.SaveChangesAsync();
    }
}
