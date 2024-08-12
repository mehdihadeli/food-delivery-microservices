using FoodDelivery.Services.Shared.Catalogs.Products.Events.v1.Integration;
using MassTransit;

namespace FoodDelivery.Services.Customers.Products.Features.CreatingProduct.V1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<ProductCreatedV1>
{
    public Task Consume(ConsumeContext<ProductCreatedV1> context)
    {
        return Task.CompletedTask;
    }
}
