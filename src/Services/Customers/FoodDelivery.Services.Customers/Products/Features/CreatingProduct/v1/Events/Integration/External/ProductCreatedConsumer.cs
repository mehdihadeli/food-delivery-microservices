using BuildingBlocks.Core.Events;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;
using MassTransit;

namespace FoodDelivery.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<EventEnvelope<ProductCreatedV1>>
{
    public Task Consume(ConsumeContext<EventEnvelope<ProductCreatedV1>> context)
    {
        return Task.CompletedTask;
    }
}
