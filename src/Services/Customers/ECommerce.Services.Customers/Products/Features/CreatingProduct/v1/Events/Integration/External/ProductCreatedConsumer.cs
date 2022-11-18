using ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;
using MassTransit;

namespace ECommerce.Services.Customers.Products.Features.CreatingProduct.v1.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        return Task.CompletedTask;
    }
}
