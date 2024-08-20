using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusPublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    Task PublishAsync<TMessage>(IEventEnvelope<TMessage> eventEnvelope, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    public Task PublishAsync<TMessage>(
        TMessage message,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage;

    public Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage;
}
