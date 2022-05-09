using BuildingBlocks.Abstractions.CQRS.Event;

namespace BuildingBlocks.Abstractions.Persistence
{
    /// <summary>
    /// Generic Repository that Implemented in All Repositories type Async Mode
    /// </summary>
    public interface IEventRepository<TContext, TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Insert single data Async Mode
        /// </summary>
        Task InsertEvent(TEvent @event, CancellationToken cancellationToken);

        /// <summary>
        /// Insert multiple data Async Mode
        /// </summary>
        Task InsertRangeEvent(List<TEvent> @events, CancellationToken cancellationToken);

        /// <summary>
        /// Update single data Async Mode
        /// </summary>
        Task UpdateEvent(TEvent @event, CancellationToken cancellationToken);

        /// <summary>
        /// Update Multiple data Async Mode
        /// </summary>
        Task UpdateRangeEvent(List<TEvent> @events, CancellationToken cancellationToken);

        /// <summary>
        /// Delete Single Data Async Mode
        /// </summary>
        Task DeleteEvent(TEvent @event, CancellationToken cancellationToken);

        /// <summary>
        /// Delete Multiple Data Async Mode
        /// </summary>
        Task DeleteRangeEvent(List<TEvent> @events, CancellationToken cancellationToken);
    }
}
