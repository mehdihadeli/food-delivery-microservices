using BuildingBlocks.Abstractions.Domain.Events.Internal;
using EventStore.Client;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class EventStoreClientExtensions
{
    public static async Task<TEntity> Find<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        string streamId,
        CancellationToken cancellationToken
    )
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        return (
            await readResult
                .Select(@event => @event.DeserializeData()!)
                .AggregateAsync(getDefault(), when, cancellationToken)
        )!;
    }

    public static async Task<ulong> Append<TEvent>(
        this EventStoreClient eventStore,
        string streamId,
        TEvent @event,
        CancellationToken cancellationToken
    )
        where TEvent : IDomainEvent
    {
        var result = await eventStore.AppendToStreamAsync(
            streamId,
            StreamState.NoStream,
            new[] { @event.ToJsonEventData() },
            cancellationToken: cancellationToken
        );
        return result.NextExpectedStreamRevision;
    }

    public static async Task<ulong> Append<TEvent>(
        this EventStoreClient eventStore,
        string streamId,
        TEvent @event,
        ulong expectedRevision,
        CancellationToken cancellationToken
    )
        where TEvent : IDomainEvent
    {
        var result = await eventStore.AppendToStreamAsync(
            streamId,
            expectedRevision,
            new[] { @event.ToJsonEventData() },
            cancellationToken: cancellationToken
        );

        return result.NextExpectedStreamRevision;
    }
}
