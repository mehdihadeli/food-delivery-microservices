using BuildingBlocks.Abstractions.CQRS.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

public class SupplierUpdatedConsumer : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdated>
{
    public Task Handle(Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
