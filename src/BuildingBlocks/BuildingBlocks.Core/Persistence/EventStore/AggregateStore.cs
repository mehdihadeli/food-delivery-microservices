using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Persistence.EventStore;

public class AggregateStore : IAggregateStore
{
    private readonly IEventStore _eventStore;
    private readonly IAggregatesDomainEventsRequestStore _aggregatesDomainEventsStore;

    public AggregateStore(IEventStore eventStore, IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    {
        _eventStore = eventStore;
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        aggregateId.NotBeNull();

        var streamName = StreamName.For<TAggregate, TId>(aggregateId);

        var defaultAggregateState = AggregateFactory<TAggregate>.CreateAggregate();

        var result = await _eventStore.AggregateStreamAsync<TAggregate, TId>(
            streamName,
            StreamReadPosition.Start,
            defaultAggregateState,
            defaultAggregateState.Fold,
            cancellationToken
        );

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
            .Select(
                x =>
                    x.ToStreamEvent(
                        new StreamEventMetadata(x.EventId.ToString(), (ulong)x.AggregateSequenceNumber, null, null)
                    )
            )
            .ToImmutableList();

        var result = await _eventStore.AppendEventsAsync(streamName, streamEvents, version, cancellationToken);

        _aggregatesDomainEventsStore.AddEvents(events);

        aggregate.MarkUncommittedDomainEventAsCommitted();

        await _eventStore.CommitAsync(cancellationToken);

        return result;
    }

    public Task<AppendResult> StoreAsync<TAggregate, TId>(
        TAggregate aggregate,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
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

        return _eventStore.StreamExists(streamName, cancellationToken);
    }
}
