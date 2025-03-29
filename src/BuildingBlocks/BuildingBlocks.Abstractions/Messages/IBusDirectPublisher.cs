namespace BuildingBlocks.Abstractions.Messages;

public interface IBusDirectPublisher
{
    Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage;

    Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default);

    public Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage;

    public Task PublishAsync(
        IMessageEnvelopeBase messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    );
}
