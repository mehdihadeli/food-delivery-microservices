using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusDirectPublisher
{
    Task PublishAsync<TMessage>(IEventEnvelope<TMessage> eventEnvelope, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default);

    public Task PublishAsync<TMessage>(
        IEventEnvelope<TMessage> eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage;

    public Task PublishAsync(
        IEventEnvelope eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    );
}
