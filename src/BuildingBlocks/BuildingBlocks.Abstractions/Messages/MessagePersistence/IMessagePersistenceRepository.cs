using System.Linq.Expressions;

namespace BuildingBlocks.Abstractions.Messages.MessagePersistence;

public interface IMessagePersistenceRepository
{
    Task AddAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default);
    Task UpdateAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default);

    Task ChangeStateAsync(Guid messageId, MessageStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersistMessage>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        Expression<Func<PersistMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PersistMessage>> GetFilterBySateAsync(
        MessageStatus? status = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PersistMessage>> GetFilterByAsync(
        MessageStatus? status = null,
        MessageDeliveryType? deliveryType = null,
        string? dataType = null,
        CancellationToken cancellationToken = default
    );

    Task<PersistMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default);

    Task CleanupMessages();
}
