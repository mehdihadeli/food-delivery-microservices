using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using Marten;

namespace BuildingBlocks.Persistence.Marten;

public class MartenEventStore(IDocumentSession documentSession) : IEventStore
{
    public Task<bool> StreamExists(string streamId, CancellationToken cancellationToken = default)
    {
        var state = documentSession.Events.FetchStreamState(streamId);

        return Task.FromResult(state != null);
    }

    public async Task<IEnumerable<IStreamEventEnvelope>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var events = await Filter(streamId, fromVersion?.Value, null).ToListAsync(cancellationToken);

        // events that we saved are IStreamEvent
        var streamEvents = events.Select(ev => ev.Data).OfType<IStreamEventEnvelope>().ToImmutableList();

        return streamEvents;
    }

    public Task<IEnumerable<IStreamEventEnvelope>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default
    )
    {
        return GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEventEnvelope @event,
        CancellationToken cancellationToken = default
    )
    {
        // storing whole stream event with metadata because there is no way to store metadata separately
        var result = documentSession.Events.Append(streamId, @event);

        var nextVersion = documentSession.Events.FetchStream(streamId).Count;

        return Task.FromResult(new AppendResult(-1, nextVersion));
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEventEnvelope @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        return AppendEventsAsync(streamId, new[] { @event }, expectedRevision: expectedRevision, cancellationToken);
    }

    public Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEventEnvelope> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        // storing whole stream event with metadata because there is no way to store metadata separately
        var result = documentSession.Events.Append(
            streamId,
            expectedVersion: expectedRevision.Value,
            events: events.Cast<object>().ToArray()
        );

        var nextVersion = expectedRevision.Value + events.Count;

        return Task.FromResult(new AppendResult(-1, nextVersion));
    }

    public Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        return documentSession.Events.AggregateStreamAsync<TAggregate>(
            streamId,
            version: fromVersion.Value,
            null,
            token: cancellationToken
        );
    }

    public Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        return documentSession.Events.AggregateStreamAsync<TAggregate>(
            streamId,
            version: StreamReadPosition.Start.Value,
            null,
            token: cancellationToken
        );
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return documentSession.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<global::Marten.Events.IEvent> Filter(string streamId, long? version, DateTime? timestamp)
    {
        var query = documentSession.Events.QueryAllRawEvents().AsQueryable();

        query = query.Where(ev => ev.StreamKey == streamId);

        if (version.HasValue)
            query = query.Where(ev => ev.Version >= version);

        if (timestamp.HasValue)
            query = query.Where(ev => ev.Timestamp >= timestamp);

        return query;
    }
}
