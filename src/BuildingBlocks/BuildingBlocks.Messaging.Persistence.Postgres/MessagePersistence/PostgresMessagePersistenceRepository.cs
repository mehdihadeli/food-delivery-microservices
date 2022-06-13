using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class PostgresMessagePersistenceRepository : IMessagePersistenceRepository
{
    private readonly MessagePersistenceDbContext _persistenceDbContext;
    private readonly ILogger<PostgresMessagePersistenceRepository> _logger;

    public PostgresMessagePersistenceRepository(
        MessagePersistenceDbContext persistenceDbContext,
        ILogger<PostgresMessagePersistenceRepository> logger)
    {
        _persistenceDbContext = persistenceDbContext;
        _logger = logger;
    }

    public async Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        await _persistenceDbContext.StoreMessages.AddAsync(storeMessage, cancellationToken);

        await _persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        _persistenceDbContext.StoreMessages.Attach(storeMessage);
        var entry = _persistenceDbContext.Entry(storeMessage);
        entry.State = EntityState.Modified;

        await _persistenceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return (await _persistenceDbContext.StoreMessages.ToListAsync(cancellationToken)).AsReadOnly();
    }

    public async Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return (await _persistenceDbContext.StoreMessages.Where(predicate).ToListAsync(cancellationToken)).AsReadOnly();
    }

    public async Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _persistenceDbContext.StoreMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> RemoveAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        _persistenceDbContext.StoreMessages.Remove(storeMessage);
        var res = await _persistenceDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task CleanupMessages()
    {
        if (await _persistenceDbContext.StoreMessages.AnyAsync() == false)
            return;

        _persistenceDbContext.StoreMessages.RemoveRange(_persistenceDbContext.StoreMessages);

        await _persistenceDbContext.SaveChangesAsync();
    }
}
