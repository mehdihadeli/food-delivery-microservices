using BuildingBlocks.Abstractions.Domain.Events;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.Integration.External;

public class SupplierCreatedConsumer
    : IEventHandler<FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierCreatedV1>
{
    public Task Handle(
        FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierCreatedV1 notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
