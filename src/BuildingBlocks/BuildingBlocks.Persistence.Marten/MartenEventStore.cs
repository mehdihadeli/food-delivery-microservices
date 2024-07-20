using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using Marten;

namespace BuildingBlocks.Persistence.Marten;

public class MartenEventStore : IEventStore
{
    private readonly IDocumentSession _documentSession;

    public MartenEventStore(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public Task<bool> StreamExists(string streamId, CancellationToken cancellationToken = default)
    {
        var state = _documentSession.Events.FetchStreamState(streamId);

        return Task.FromResult(state != null);
    }

    public async Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var events = await Filter(streamId, fromVersion?.Value, null).ToListAsync(cancellationToken);

        // events that we saved are IStreamEvent
        var streamEvents = events.Select(ev => ev.Data).OfType<IStreamEvent>().ToImmutableList();

        return streamEvents;
    }

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default
    )
    {
        return GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        // storing whole stream event with metadata because there is no way to store metadata separately
        var result = _documentSession.Events.Append(streamId, @event);

        var nextVersion = _documentSession.Events.FetchStream(streamId).Count;

        return Task.FromResult(new AppendResult(-1, nextVersion));
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        return AppendEventsAsync(streamId, new[] { @event }, expectedRevision: expectedRevision, cancellationToken);
    }

    public Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEvent> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        // storing whole stream event with metadata because there is no way to store metadata separately
        var result = _documentSession.Events.Append(
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
        return _documentSession.Events.AggregateStreamAsync<TAggregate>(
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
        return _documentSession.Events.AggregateStreamAsync<TAggregate>(
            streamId,
            version: StreamReadPosition.Start.Value,
            null,
            token: cancellationToken
        );
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _documentSession.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<global::Marten.Events.IEvent> Filter(string streamId, long? version, DateTime? timestamp)
    {
        var query = _documentSession.Events.QueryAllRawEvents().AsQueryable();

        query = query.Where(ev => ev.StreamKey == streamId);

        if (version.HasValue)
            query = query.Where(ev => ev.Version >= version);

        if (timestamp.HasValue)
            query = query.Where(ev => ev.Timestamp >= timestamp);

        return query;
    }
}
