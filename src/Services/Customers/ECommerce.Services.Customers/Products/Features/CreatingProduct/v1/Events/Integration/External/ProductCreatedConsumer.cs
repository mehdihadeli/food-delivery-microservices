using ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;
using MassTransit;

namespace ECommerce.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<ProductCreatedV1>
{
    public Task Consume(ConsumeContext<ProductCreatedV1> context)
    {
        return Task.CompletedTask;
    }
}
