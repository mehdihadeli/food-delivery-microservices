using MassTransit;
using Store.Services.Shared.Customers.Customers.Events.Integration;

namespace Store.Services.Customers.Customers.Features.CreatingCustomer.Events.Integration;

public class CustomerCreatedConsumer : IConsumer<CustomerCreated>
{
    public Task Consume(ConsumeContext<CustomerCreated> context)
    {
        return Task.CompletedTask;
    }
}
