using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;

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
            CustomerCreated customerCreated => CustomerCreatedV1.Of(customerCreated.Id),
            CustomerUpdated customerCreated => CustomerUpdatedV1.Of(
                customerCreated.Id,
                customerCreated.FirstName,
                customerCreated.LastName,
                customerCreated.Email,
                customerCreated.PhoneNumber,
                customerCreated.IdentityId,
                customerCreated.OccurredOn,
                customerCreated.BirthDate,
                customerCreated.Nationality,
                customerCreated.DetailAddress
            ),
            _ => null,
        };
    }
}
