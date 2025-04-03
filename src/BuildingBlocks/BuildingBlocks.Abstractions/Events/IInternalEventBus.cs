using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Abstractions.Events;

public interface IInternalEventBus
{
    /// <summary>
    ///     publish an in-memory event.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(T eventData, CancellationToken ct)
        where T : class, IEvent;

    /// <summary>
    ///     publish multiple in-memory events.
    /// </summary>
    /// <param name="eventsData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IEnumerable<T> eventsData, CancellationToken ct)
        where T : class, IEvent;

    /// <summary>
    ///     publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <param name="messageEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IMessageEnvelopeBase messageEnvelope, CancellationToken ct);

    /// <summary>
    ///     publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="messageEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IMessageEnvelope<T> messageEnvelope, CancellationToken ct)
        where T : class, IMessage;

    /// <summary>
    ///     publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <param name="streamEvent"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IStreamEventEnvelopeBase streamEvent, CancellationToken ct);

    /// <summary>
    ///     publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="streamEvent"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken ct)
        where T : class, IDomainEvent;
}
