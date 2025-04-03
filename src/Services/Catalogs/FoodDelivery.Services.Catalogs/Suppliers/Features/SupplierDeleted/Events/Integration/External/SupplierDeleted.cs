using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using MassTransit;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;

public class SupplierDeletedConsumer : IConsumer<SupplierDeletedV1>
{
    public Task Consume(ConsumeContext<SupplierDeletedV1> context)
    {
        return Task.CompletedTask;
    }
}
