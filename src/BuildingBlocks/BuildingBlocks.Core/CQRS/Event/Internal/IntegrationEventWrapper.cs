using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Core.CQRS.Event.Internal;

public record IntegrationEventWrapper<TDomainEventType>
    : IntegrationEvent
    where TDomainEventType : IDomainEvent;
