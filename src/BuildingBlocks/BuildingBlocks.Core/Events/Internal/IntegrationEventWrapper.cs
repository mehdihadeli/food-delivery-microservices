using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Messages;

namespace BuildingBlocks.Core.Events.Internal;

public record IntegrationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : IntegrationEvent
    where TDomainEventType : IDomainEvent;
