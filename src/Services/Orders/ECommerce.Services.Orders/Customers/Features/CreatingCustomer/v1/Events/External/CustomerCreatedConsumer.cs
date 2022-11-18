using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using MassTransit;

namespace ECommerce.Services.Orders.Customers.Features.CreatingCustomer.v1.Events.External;

public class CustomerCreatedConsumer : IConsumer<CustomerCreated>
{
    public Task Consume(ConsumeContext<CustomerCreated> context)
    {
        return Task.CompletedTask;
    }
}
