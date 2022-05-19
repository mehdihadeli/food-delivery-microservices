using BuildingBlocks.Core.CQRS.Events.Internal;

namespace ECommerce.Services.Customers;

public record TestDomainEvent(string Data) : DomainEvent;
