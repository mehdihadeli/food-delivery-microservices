using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;
using MassTransit;

namespace FoodDelivery.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<IEventEnvelope<ProductCreatedV1>>
{
    public Task Consume(ConsumeContext<IEventEnvelope<ProductCreatedV1>> context)
    {
        return Task.CompletedTask;
    }
}
