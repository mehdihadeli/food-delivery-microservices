using ECommerce.Services.Shared.Catalogs.Products.Events.Integration;
using MassTransit;

namespace ECommerce.Services.Customers.Products.Features.CreatingProduct.Events.Integration.External;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        return Task.CompletedTask;
    }
}
