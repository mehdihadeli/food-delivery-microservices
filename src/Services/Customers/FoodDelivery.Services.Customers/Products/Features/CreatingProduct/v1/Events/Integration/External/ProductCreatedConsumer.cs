using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using MassTransit;

namespace FoodDelivery.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<ProductCreatedV1>
{
    public Task Consume(ConsumeContext<ProductCreatedV1> context)
    {
        return Task.CompletedTask;
    }
}
