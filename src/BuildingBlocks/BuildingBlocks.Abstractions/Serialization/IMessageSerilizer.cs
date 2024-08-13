using BuildingBlocks.Abstractions.Events;

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

    /// <summary>
    /// Deserialize the given payload into a <see cref="IEventEnvelope" />.
    /// </summary>
    /// <param name="eventEnvelope">a json data to deserialize to a messageEnvelope.</param>
    /// <returns>return a messageEnvelope type.</returns>
    IEventEnvelope? Deserialize(string eventEnvelope);
    IEventEnvelope? Deserialize(string eventEnvelope, Type messageType);
}
