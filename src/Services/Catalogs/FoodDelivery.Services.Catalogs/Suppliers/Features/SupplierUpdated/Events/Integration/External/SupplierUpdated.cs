using BuildingBlocks.Abstractions.Domain.Events;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

public class SupplierUpdatedConsumer
    : IEventHandler<FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdatedV1>
{
    public Task Handle(
        FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdatedV1 notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
