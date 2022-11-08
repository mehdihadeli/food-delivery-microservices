using BuildingBlocks.Abstractions.CQRS.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.Integration.External;

public class SupplierCreatedConsumer : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierCreated>
{
    public Task Handle(Services.Shared.Catalogs.Suppliers.Events.Integration.SupplierCreated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
