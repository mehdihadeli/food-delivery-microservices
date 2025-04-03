using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Messages;
using MassTransit;

namespace BuildingBlocks.Core.Events.Internal;

[ExcludeFromTopology]
public record IntegrationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : IntegrationEvent
    where TDomainEventType : IDomainEvent;
