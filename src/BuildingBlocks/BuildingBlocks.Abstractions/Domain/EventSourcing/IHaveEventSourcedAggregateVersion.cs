namespace BuildingBlocks.Abstractions.Domain.EventSourcing;

public interface IHaveEventSourcedAggregateVersion : IHaveAggregateVersion
{
    /// <summary>
    /// The current version is set to original version when the aggregate is loaded from the store.
    /// It should increase for each state transition performed within the scope of the current operation.
    /// </summary>
    long CurrentVersion { get; }
}
