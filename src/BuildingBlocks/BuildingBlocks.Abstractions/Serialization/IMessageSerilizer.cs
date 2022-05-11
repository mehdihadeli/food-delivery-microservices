using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Abstractions.Serialization;

public interface IMessageSerializer : ISerializer
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
    /// Deserialize the given string into a <see cref="MessageEnvelope"/>
    /// </summary>
    /// <param name="json">a json data to deserialize to a messageEnvelope.</param>
    /// <returns>return a messageEnvelope type.</returns>
    MessageEnvelope? Deserialize(string json);

    /// <summary>
    /// Deserialize the given byte array back into a message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="payloadType"></param>
    /// <returns></returns>
    IMessage? Deserialize(ReadOnlySpan<byte> data, string payloadType);

    /// <summary>
    ///  Deserialize the given string into a <see cref="TMessage"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    TMessage? Deserialize<TMessage>(string message)
        where TMessage : IMessage;

    /// <summary>
    /// Deserialize the given string into a object.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="payloadType"></param>
    /// <returns></returns>
    object? Deserialize(string payload, string payloadType);
}
