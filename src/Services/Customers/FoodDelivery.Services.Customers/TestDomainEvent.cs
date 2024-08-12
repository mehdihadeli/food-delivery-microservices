using BuildingBlocks.Core.Domain.Events.Internal;

namespace FoodDelivery.Services.Customers;

public record TestDomainEvent(string Data) : DomainEvent
{
    public override bool Equals(object obj)
    {
        return Equals(obj as TestDomainEvent);
    }
}
