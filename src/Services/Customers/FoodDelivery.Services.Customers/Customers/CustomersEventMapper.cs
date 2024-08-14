using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Shared.Customers.Customers.Events.V1.Integration;

namespace FoodDelivery.Services.Customers.Customers;

public class CustomersEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList();
    }

    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            TestDomainEvent e => new TestIntegration(e.Data),
            CustomerCreated customerCreated => CustomerCreatedV1.Of(customerCreated.Id),
            CustomerUpdated customerCreated
                => CustomerUpdatedV1.Of(
                    customerCreated.Id,
                    customerCreated.FirstName,
                    customerCreated.LastName,
                    customerCreated.Email,
                    customerCreated.PhoneNumber,
                    customerCreated.IdentityId,
                    customerCreated.CreatedAt,
                    customerCreated.BirthDate,
                    customerCreated.Nationality,
                    customerCreated.DetailAddress
                ),
            _ => null
        };
    }
}
