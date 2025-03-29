namespace BuildingBlocks.Core.Messages.MessagePersistence.InMemory;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Extensions;

public class InMemoryMessagePersistenceRepository : IMessagePersistenceRepository
{
    private static readonly ConcurrentDictionary<Guid, PersistMessage> _messages = new();

    public Task AddAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        persistMessage.NotBeNull();

        _messages.TryAdd<Guid, PersistMessage>(persistMessage.Id, persistMessage);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        if (_messages.ContainsKey(persistMessage.Id))
        {
            _messages[persistMessage.Id] = persistMessage;
        }

        return Task.CompletedTask;
    }

    public Task ChangeStateAsync(Guid messageId, MessageStatus status, CancellationToken cancellationToken = default)
    {
        _messages.TryGetValue(messageId, out PersistMessage? message);

        if (message is { })
        {
            message.ChangeState(status);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PersistMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = _messages.Select(x => x.Value).ToImmutableList();

        return Task.FromResult<IReadOnlyList<PersistMessage>>(result);
    }

    public Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        Expression<Func<PersistMessage, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        predicate.NotBeNull();

        var result = _messages.Select(x => x.Value).Where(predicate.Compile()).ToImmutableList();

        return Task.FromResult<IReadOnlyList<PersistMessage>>(result);
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

    public Task<PersistMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = _messages.FirstOrDefault(x => x.Key == id).Value;

        return Task.FromResult(result)!;
    }

    public Task<bool> RemoveAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
    {
        persistMessage.NotBeNull();

        var result = _messages.Remove(persistMessage.Id, out _);

        return Task.FromResult(result)!;
    }

    public Task CleanupMessages()
    {
        foreach (var storeMessage in _messages)
        {
            _messages.TryRemove(storeMessage);
        }

        return Task.CompletedTask;
    }
}
