namespace BuildingBlocks.Abstractions.Messages;

public interface IBusPublisher
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage;

    Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default);

    Task PublishAsync<TMessage>(
        TMessage message,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage;

    Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage;
}
