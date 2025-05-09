using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;

namespace BuildingBlocks.Core.Persistence.EventStore;


public abstract class AggregateStore(
    IEventStore eventStore,
    IAggregatesDomainEventsRequestStorage aggregatesDomainEventsStorage
) : IAggregateStore
{
    public async Task<TAggregate?> GetAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        aggregateId.NotBeNull();

        var streamName = StreamName.For<TAggregate, TId>(aggregateId);

        var defaultAggregateState = AggregateFactory<TAggregate>.CreateAggregate();

        var result = await eventStore
            .AggregateStreamAsync<TAggregate, TId>(
                streamName,
                StreamReadPosition.Start,
                defaultAggregateState,
                defaultAggregateState.Fold,
                cancellationToken
            )
            .ConfigureAwait(false);

        return result;
    }

    public async Task<AppendResult> StoreAsync<TAggregate, TId>(
        TAggregate aggregate,
        ExpectedStreamVersion? expectedVersion = null,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        aggregate.NotBeNull();

        var streamName = StreamName.For<TAggregate, TId>(aggregate.Id);

        ExpectedStreamVersion version = expectedVersion ?? new ExpectedStreamVersion(aggregate.OriginalVersion);

        var events = aggregate.GetUncommittedDomainEvents();

        // update events aggregateId and event versions
        foreach (var item in events.Select((value, i) => new { index = i, value }))
        {
            item.value.WithAggregate(aggregate.Id, aggregate.CurrentVersion + (item.index + 1));
        }

        var streamEvents = events
            .Select(domainEvent =>
                StreamEventEnvelopeFactory.From(
                    domainEvent,
                    new StreamEventMetadata(
                        domainEvent.EventId.ToString(),
                        (ulong)domainEvent.AggregateSequenceNumber,
                        null
                    )
                )
            )
            .ToImmutableList();

        var result = await eventStore
            .AppendEventsAsync(streamName, streamEvents, version, cancellationToken)
            .ConfigureAwait(false);

        aggregatesDomainEventsStorage.AddEventsFromAggregate(aggregate);

        await eventStore.CommitAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    public Task<AppendResult> StoreAsync<TAggregate, TId>(
        TAggregate aggregate,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        aggregate.NotBeNull();

        return StoreAsync<TAggregate, TId>(
            aggregate,
            new ExpectedStreamVersion(aggregate.OriginalVersion),
            cancellationToken
        );
    }

    public Task<bool> Exists<TAggregate, TId>(TId aggregateId, CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        aggregateId.NotBeNull();

        var streamName = StreamName.For<TAggregate, TId>(aggregateId);

        return eventStore.StreamExists(streamName, cancellationToken);
    }
}
