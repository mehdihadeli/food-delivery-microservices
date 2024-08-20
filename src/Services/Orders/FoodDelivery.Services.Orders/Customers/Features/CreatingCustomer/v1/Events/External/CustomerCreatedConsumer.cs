using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Shared.Customers.Customers.Events.V1.Integration;
using MassTransit;

namespace FoodDelivery.Services.Orders.Customers.Features.CreatingCustomer.V1.Events.External;

public class CustomerCreatedConsumer : IConsumer<IEventEnvelope<CustomerCreatedV1>>
{
    public Task Consume(ConsumeContext<IEventEnvelope<CustomerCreatedV1>> context)
    {
        return Task.CompletedTask;
    }
}
