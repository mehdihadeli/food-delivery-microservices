using BuildingBlocks.Core.Events.Internal;

namespace FoodDelivery.Services.Customers;

public record TestDomainEvent(string Data) : DomainEvent;
