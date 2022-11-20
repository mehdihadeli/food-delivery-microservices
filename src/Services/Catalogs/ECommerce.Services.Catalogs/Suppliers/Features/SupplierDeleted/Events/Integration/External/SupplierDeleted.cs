using BuildingBlocks.Abstractions.CQRS.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;
public class SupplierDeletedConsumer : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierDeletedV1>
{
    public Task Handle(Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierDeletedV1 notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
