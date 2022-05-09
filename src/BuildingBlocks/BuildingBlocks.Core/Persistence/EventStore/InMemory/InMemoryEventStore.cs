using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Core.Persistence.EventStore.InMemory;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, InMemoryStream> _storage = new();
    private readonly List<StreamEventData> _global = new();

    public Task<bool> StreamExists(string streamId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(streamId));
    }

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var result = FindStream(streamId).GetEvents(fromVersion ?? StreamReadPosition.Start, maxCount);

        return Task.FromResult<IEnumerable<IStreamEvent>>(result.Select(x => x.ToStreamEvent()));
    }

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default)
    {
        return GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        CancellationToken cancellationToken = default)
    {
        return AppendEventsAsync(streamId, new[] { @event }, ExpectedStreamVersion.NoStream, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        return AppendEventsAsync(streamId, new[] { @event }, expectedRevision, cancellationToken);
    }

    public Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEvent> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
            _storage.Add(streamId, existing);
        }

        var inMemoryEvents = events.Select(x => x.ToJsonStreamEventData()).ToList();

        existing.AppendEvents(expectedRevision, _global.Count - 1, inMemoryEvents);

        _global.AddRange(inMemoryEvents);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        // var streamEvents = (await GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken)).Select(x => x.Data);
        var streamEvents = FindStream(streamId).GetEvents(fromVersion, int.MaxValue).Select(x => x.DeserializeData());

        var result = streamEvents.Aggregate(
            defaultAggregateState,
            (agg, @event) =>
            {
                fold(@event);
                return agg;
            }
        );

        return Task.FromResult(result);
    }

    public Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        return AggregateStreamAsync<TAggregate, TId>(
            streamId,
            StreamReadPosition.Start,
            defaultAggregateState,
            fold,
            cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private InMemoryStream FindStream(string stream)
    {
        if (!_storage.TryGetValue(stream, out var existing))
            throw new System.Exception($"Stream with name: {stream} not found.");

        return existing;
    }
}
