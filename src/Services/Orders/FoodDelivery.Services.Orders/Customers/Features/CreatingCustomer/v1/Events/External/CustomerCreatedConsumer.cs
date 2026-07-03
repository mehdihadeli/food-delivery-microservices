using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;

namespace FoodDelivery.Services.Orders.Customers.Features.CreatingCustomer.v1.Events.External;

[AsyncApi]
public class CustomerCreatedConsumer(HeaderPropagationValues headerPropagationValues)
{
    public Task Handle(CustomerCreatedV1 message, Envelope envelope)
    {
        return WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            _ => Task.CompletedTask
        );
    }
}
