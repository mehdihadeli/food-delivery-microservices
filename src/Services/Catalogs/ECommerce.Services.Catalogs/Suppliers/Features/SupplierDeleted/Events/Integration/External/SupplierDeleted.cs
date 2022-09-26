using BuildingBlocks.Abstractions.CQRS.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;
public class SupplierDeletedConsumer : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierDeleted>
{
    public Task Handle(Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierDeleted notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
