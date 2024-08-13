using BuildingBlocks.Abstractions.Events;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.V1.Integration;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.Integration.External;

public class SupplierCreatedConsumer : IEventHandler<SupplierCreatedV1>
{
    public Task Handle(SupplierCreatedV1 notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
