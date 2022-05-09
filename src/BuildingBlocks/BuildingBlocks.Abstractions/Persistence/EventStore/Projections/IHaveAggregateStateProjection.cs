namespace BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

public interface IHaveAggregateStateProjection
{
    /// <summary>
    /// Update the aggregate state with new events that are added to the event store and also for events that are already in the event store without increasing the version.
    /// </summary>
    /// <param name="event"></param>
    void When(object @event);

    /// <summary>
    /// Restore the aggregate state with events that are loaded form the event store and increase the current version and last commit version.
    /// </summary>
    /// <param name="event"></param>
    void Fold(object @event);
}
