using BuildingBlocks.Abstractions.CQRS.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

public class SupplierUpdatedConsumer : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierUpdated>
{
    public Task Handle(Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierUpdated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
