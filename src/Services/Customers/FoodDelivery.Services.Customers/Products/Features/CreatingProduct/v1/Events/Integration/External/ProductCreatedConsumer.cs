using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;

namespace FoodDelivery.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

[AsyncApi]
public class ProductCreatedConsumer(HeaderPropagationValues headerPropagationValues)
{
    public Task Handle(ProductCreatedV1 message, Envelope envelope)
    {
        return WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            _ => Task.CompletedTask
        );
    }
}
