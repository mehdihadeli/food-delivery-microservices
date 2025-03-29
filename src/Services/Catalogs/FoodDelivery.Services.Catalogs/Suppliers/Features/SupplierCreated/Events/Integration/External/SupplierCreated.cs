using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using MassTransit;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.Integration.External;

public class SupplierCreatedConsumer : IConsumer<SupplierCreatedV1>
{
    public Task Consume(ConsumeContext<SupplierCreatedV1> context)
    {
        return Task.CompletedTask;
    }
}
