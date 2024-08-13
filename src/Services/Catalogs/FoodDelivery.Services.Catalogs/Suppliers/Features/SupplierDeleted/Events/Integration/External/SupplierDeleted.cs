using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.V1.Integration;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;

public class SupplierDeletedConsumer : IEventHandler<SupplierDeletedV1>
{
    public Task Handle(SupplierDeletedV1 notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
