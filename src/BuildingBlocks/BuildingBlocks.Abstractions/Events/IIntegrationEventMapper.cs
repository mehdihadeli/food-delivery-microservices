using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Abstractions.Events;

public interface IIntegrationEventMapper
{
    IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent);
}
