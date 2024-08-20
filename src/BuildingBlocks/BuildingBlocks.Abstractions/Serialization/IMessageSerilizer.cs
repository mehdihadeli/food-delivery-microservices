using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Abstractions.Serialization;

public interface IMessageSerializer
{
    string ContentType { get; }

    /// <summary>
    /// Serializes the given <see cref="IEventEnvelope" /> into a string.
    /// </summary>
    /// <param name="eventEnvelope">a messageEnvelope that implement IMessage interface.</param>
    /// <returns>a json string for serialized messageEnvelope.</returns>
    string Serialize(IEventEnvelope eventEnvelope);
    string Serialize<T>(IEventEnvelope<T> eventEnvelope)
        where T : IMessage;

    /// <summary>
    /// Deserialize the given payload into a <see cref="IEventEnvelope" />.
    /// </summary>
    /// <param name="eventEnvelope">a json data to deserialize to a messageEnvelope.</param>
    /// <param name="messageType">the type of message inside event-envelope.</param>
    /// <returns>return a messageEnvelope type.</returns>
    IEventEnvelope? Deserialize(string eventEnvelope, Type messageType);

    /// <summary>
    /// Deserialize the given payload into a IEventEnvelope" />.
    /// </summary>
    /// <param name="eventEnvelope"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEventEnvelope<T>? Deserialize<T>(string eventEnvelope)
        where T : IMessage;
}
