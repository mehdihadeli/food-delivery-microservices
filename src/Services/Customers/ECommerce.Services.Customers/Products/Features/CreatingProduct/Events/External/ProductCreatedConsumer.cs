using MassTransit;
using ECommerce.Services.Shared.Catalogs.Products.Events.Integration;

namespace ECommerce.Services.Customers.Products.Features.CreatingProduct.Events.External;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        return Task.CompletedTask;
    }
}
