using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using MassTransit;
using Saunter.Attributes;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

[AsyncApi]
public class SupplierUpdatedConsumer : IConsumer<SupplierUpdatedV1>
{
    public Task Consume(ConsumeContext<SupplierUpdatedV1> context)
    {
        return Task.CompletedTask;
    }
}
