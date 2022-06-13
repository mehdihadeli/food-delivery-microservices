using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;

namespace BuildingBlocks.Core.Messaging.MessagePersistence;

public class NullPersistenceRepository : IMessagePersistenceRepository
{
    public Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return new Task<IReadOnlyList<StoreMessage>>(null);
    }

    public Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return new Task<IReadOnlyList<StoreMessage>>(null);
    }

    public Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return new Task<StoreMessage?>(null);
    }

    public Task<bool> RemoveAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task CleanupMessages()
    {
        return Task.CompletedTask;
    }
}
