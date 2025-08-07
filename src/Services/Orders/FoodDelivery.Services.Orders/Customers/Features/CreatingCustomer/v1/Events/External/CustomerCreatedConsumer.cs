using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using MassTransit;
using Saunter.Attributes;

namespace FoodDelivery.Services.Orders.Customers.Features.CreatingCustomer.v1.Events.External;

[AsyncApi]
public class CustomerCreatedConsumer : IConsumer<CustomerCreatedV1>
{
    public Task Consume(ConsumeContext<CustomerCreatedV1> context)
    {
        return Task.CompletedTask;
    }
}
