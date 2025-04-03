using Mediator;

namespace BuildingBlocks.Abstractions.Events;

/// <summary>
///     The event interface.
/// </summary>
public interface IEvent : INotification
{
    /// <summary>
    ///     Gets the event identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    ///     Gets the event/aggregate root version.
    /// </summary>
    long EventVersion { get; }

    /// <summary>
    ///     Gets the date the <see cref="IEvent" /> occurred on.
    /// </summary>
    DateTime OccurredOn { get; }

    DateTimeOffset TimeStamp { get; }

    /// <summary>
    ///     Gets type of this event.
    /// </summary>
    public string EventType { get; }
}
