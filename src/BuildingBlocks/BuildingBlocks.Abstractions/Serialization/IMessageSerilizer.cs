using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Abstractions.Serialization;

public interface IMessageSerializer
{
    string ContentType { get; }

    /// <summary>
    /// Serializes the given <see cref="MessageEnvelope"/> into a string
    /// </summary>
    /// <param name="messageEnvelope">a messageEnvelope that implement IMessage interface.</param>
    /// <returns>a json string for serialized messageEnvelope.</returns>
    string Serialize(MessageEnvelope messageEnvelope);

    string Serialize<TMessage>(TMessage message)
        where TMessage : IMessage;

    /// <summary>
    /// Serializes the given <see cref="MessageEnvelope"/> into a <see cref="TransportMessage"/>
    /// </summary>
    /// <param name="messageEnvelope">a messageEnvelope that implement IMessage interface.</param>
    /// <returns>a transport messageEnvelope.</returns>
    TransportMessage SerializeToTransportMessage(MessageEnvelope messageEnvelope);

    /// <summary>
    /// Deserialize the given string into a <see cref="MessageEnvelope"/>
    /// </summary>
    /// <param name="json">a json data to deserialize to a messageEnvelope.</param>
    /// <returns>return a messageEnvelope type.</returns>
    MessageEnvelope? Deserialize(string json);

    /// <summary>
    /// Deserialize the given <see cref="TransportMessage"/> back into a <see cref="MessageEnvelope"/>
    /// </summary>
    /// <param name="transportMessage"></param>
    /// <param name="payloadType"></param>
    /// <returns></returns>
    MessageEnvelope Deserialize(TransportMessage transportMessage, Type? payloadType);

    /// <summary>
    /// Deserialize the given byte array back into a message
    /// </summary>
    /// <param name="data"></param>
    /// <param name="payloadType"></param>
    /// <returns></returns>
    IMessage? Deserialize(ReadOnlySpan<byte> data, string payloadType);

    TMessage? Deserialize<TMessage>(string message)
        where TMessage : IMessage;
}
