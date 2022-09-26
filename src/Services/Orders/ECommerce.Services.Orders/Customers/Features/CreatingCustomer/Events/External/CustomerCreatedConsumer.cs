using ECommerce.Services.Shared.Customers.Customers.Events.Integration;
using MassTransit;

namespace ECommerce.Services.Orders.Customers.Features.CreatingCustomer.Events.External;

public class CustomerCreatedConsumer : IConsumer<CustomerCreated>
{
    public Task Consume(ConsumeContext<CustomerCreated> context)
    {
        return Task.CompletedTask;
    }
}
