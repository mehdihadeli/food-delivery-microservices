using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messages.MessagePersistence;

// Ref: http://www.kamilgrzybek.com/design/the-outbox-pattern/
// Ref: https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/
// Ref: https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/
// Ref: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/subscribe-events#designing-atomicity-and-resiliency-when-publishing-to-the-event-bus
// Ref: https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
// Ref: https://learn.microsoft.com/en-us/azure/service-bus-messaging/duplicate-detection
// Ref: https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions#receive-modes
// https://exactly-once.github.io/posts/exactly-once-delivery/
public interface IMessagePersistenceService
{
    Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        Expression<Func<PersistMessage, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
        MessageStatus? status = null,
        MessageDeliveryType? deliveryType = null,
        string? dataType = null,
        CancellationToken cancellationToken = default
    );

    Task AddPublishMessageAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default);

    Task AddReceivedMessageAsync<TMessage>(
        IMessageEnvelopeBase messageEnvelope,
        Func<IMessageEnvelopeBase, Task> dispatchAction,
        CancellationToken cancellationToken = default
    );

    Task AddInternalMessageAsync<TInternalCommand>(
        TInternalCommand internalCommand,
        CancellationToken cancellationToken = default
    )
        where TInternalCommand : IInternalCommand;

    Task AddNotificationAsync<TDomainNotification>(
        TDomainNotification notification,
        CancellationToken cancellationToken = default
    )
        where TDomainNotification : IDomainNotificationEvent<IDomainEvent>;

    Task MarkAsDeliveredAsync(Guid messageId, CancellationToken cancellationToken);
    Task ProcessAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task ProcessAllAsync(CancellationToken cancellationToken = default);
}
