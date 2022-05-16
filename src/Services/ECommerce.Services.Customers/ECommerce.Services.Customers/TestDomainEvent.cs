using BuildingBlocks.Core.CQRS.Event.Internal;

namespace ECommerce.Services.Customers;

public record TestDomainEvent(string Data) : DomainEvent;
