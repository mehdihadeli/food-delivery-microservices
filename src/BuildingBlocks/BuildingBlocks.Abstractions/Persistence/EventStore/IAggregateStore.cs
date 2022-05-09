using BuildingBlocks.Abstractions.Domain.EventSourcing;

namespace BuildingBlocks.Abstractions.Persistence.EventStore;

/// <summary>
/// This AggregateStore act like a repository for the AggregateRoot.
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Load the aggregate from the store with a aggregate id
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TId">Type of Id.</typeparam>
    /// <param name="aggregateId">Id of aggregate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with aggregate as result.</returns>
    Task<TAggregate?> GetAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();

    /// <summary>
    /// Store an aggregate state to the store with using some events (use for updating, adding and deleting).
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TId">Type of Id.</typeparam>
    /// <param name="aggregate">Aggregate object to be saved.</param>
    /// <param name="expectedVersion">Expected version saved from earlier. -1 if new.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task of operation.</returns>
    Task<AppendResult> StoreAsync<TAggregate, TId>(
        TAggregate aggregate,
        ExpectedStreamVersion? expectedVersion = null,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();

    /// <summary>
    /// Store an aggregate state to the store with using some events (use for updating, adding and deleting).
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TId">Type of Id.</typeparam>
    /// <param name="aggregate">Aggregate object to be saved.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task of operation.</returns>
    Task<AppendResult> StoreAsync<TAggregate, TId>(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();

    /// <summary>
    /// Check if aggregate exists in the store.
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <returns></returns>
    Task<bool> Exists<TAggregate, TId>(TId aggregateId, CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>, new();
}
