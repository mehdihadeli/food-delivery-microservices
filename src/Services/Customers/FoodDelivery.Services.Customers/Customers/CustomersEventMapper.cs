using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;

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
            CustomerCreated customerCreated
                => Services.Shared.Customers.Customers.Events.v1.Integration.CustomerCreatedV1.Of(customerCreated.Id),
            CustomerUpdated customerCreated
                => Services.Shared.Customers.Customers.Events.v1.Integration.CustomerUpdatedV1.Of(
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
