using BuildingBlocks.Abstractions.Domain.Events;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;

public class SupplierDeletedConsumer
    : IEventHandler<FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierDeletedV1>
{
    public Task Handle(
        FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierDeletedV1 notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
