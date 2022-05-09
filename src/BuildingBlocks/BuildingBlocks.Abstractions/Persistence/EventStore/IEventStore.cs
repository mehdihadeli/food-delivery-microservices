using BuildingBlocks.Abstractions.Domain.EventSourcing;

namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public interface IEventStore
{
    /// <summary>
    /// Check if specific stream exists in the store
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> StreamExists(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for an specific stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="fromVersion">All events after this should be returned.</param>
    /// <param name="maxCount">Number of items to read.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with events for aggregate.</returns>
    Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for an specific stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="fromVersion">All events after this should be returned.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with events for aggregate.</returns>
    Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Append event to aggregate with no stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="event">domain event to append the aggregate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Append event to aggregate with a existing or none existing stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="event">domain event to append the aggregate.</param>
    /// <param name="expectedRevision"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Append events to aggregate with a existing or none existing stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="events">domain event to append the aggregate.</param>
    /// <param name="expectedRevision"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEvent> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rehydrating aggregate from events in the event store.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="fromVersion"></param>
    /// <param name="defaultAggregateState">Initial state of the aggregate.</param>
    /// <param name="fold"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <returns></returns>
    Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();

    /// <summary>
    ///  Rehydrating aggregate from events in the event store.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="defaultAggregateState">Initial state of the aggregate.</param>
    /// <param name="fold"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <returns></returns>
    Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();

    /// <summary>
    /// Commit events to the event store.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
