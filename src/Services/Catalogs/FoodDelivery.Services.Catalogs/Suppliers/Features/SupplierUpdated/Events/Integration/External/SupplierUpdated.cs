using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.V1.Integration;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

public class SupplierUpdatedConsumer : IEventHandler<SupplierUpdatedV1>
{
    public Task Handle(SupplierUpdatedV1 notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
