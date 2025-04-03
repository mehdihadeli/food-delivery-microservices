namespace BuildingBlocks.Abstractions.Messages;

public interface IMessageEnvelope<out T> : IMessageEnvelopeBase
    where T : class, IMessage
{
    new T Message { get; }
}

// not required because of existing IMessageEnvelopeBase
// public interface IMessageEnvelope : IMessageEnvelope<object>;
