using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Abstractions.Serialization;

public interface IMessageSerializer
{
    string ContentType { get; }

    /// <summary>
    /// Serializes the given <see cref="IMessageEnvelopeBase" /> into a string.
    /// </summary>
    /// <param name="iMessageEnvelope">a messageEnvelope that implement IMessage interface.</param>
    /// <returns>a json string for serialized messageEnvelope.</returns>
    string Serialize(IMessageEnvelopeBase iMessageEnvelope);
    string Serialize<T>(IMessageEnvelope<T> messageEnvelope)
        where T : class, IMessage;

    /// <summary>
    /// Deserialize the given payload into a <see cref="IMessageEnvelopeBase" />.
    /// </summary>
    /// <param name="eventEnvelope">a json data to deserialize to a messageEnvelope.</param>
    /// <param name="messageType">the type of message inside event-envelope.</param>
    /// <returns>return a messageEnvelope type.</returns>
    IMessageEnvelopeBase? Deserialize(string eventEnvelope, Type? messageType);

    /// <summary>
    /// Deserialize the given payload into a IMessageEnvelope" />.
    /// </summary>
    /// <param name="eventEnvelope"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IMessageEnvelope<T>? Deserialize<T>(string eventEnvelope)
        where T : class, IMessage;
}
