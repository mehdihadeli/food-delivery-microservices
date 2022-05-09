using BuildingBlocks.Core.CQRS.Event.Internal;

namespace Store.Services.Customers;

public record TestDomainEvent(string Data) : DomainEvent;
