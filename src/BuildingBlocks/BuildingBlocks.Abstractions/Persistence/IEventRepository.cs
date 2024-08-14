using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Persistence;

/// <summary>
///     Generic Repository that Implemented in All Repositories type Async Mode.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface IEventRepository<TContext, TEvent>
    where TEvent : IEvent
{
    /// <summary>
    ///     Insert single data Async Mode.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InsertEvent(TEvent @event, CancellationToken cancellationToken);

    /// <summary>
    ///     Insert multiple data Async Mode.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InsertRangeEvent(IList<TEvent> events, CancellationToken cancellationToken);

    /// <summary>
    ///     Update single data Async Mode.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateEvent(TEvent @event, CancellationToken cancellationToken);

    /// <summary>
    ///     Update Multiple data Async Mode.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateRangeEvent(IList<TEvent> events, CancellationToken cancellationToken);

    /// <summary>
    ///     Delete Single Data Async Mode.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteEvent(TEvent @event, CancellationToken cancellationToken);

    /// <summary>
    ///     Delete Multiple Data Async Mode.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteRangeEvent(IList<TEvent> events, CancellationToken cancellationToken);
}
