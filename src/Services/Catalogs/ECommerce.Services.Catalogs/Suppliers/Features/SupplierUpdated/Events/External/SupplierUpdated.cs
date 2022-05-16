using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.External;

public record SupplierUpdated(long Id, string Name) : IntegrationEvent;


public class SupplierUpdatedConsumer : IEventHandler<SupplierUpdated>
{
    public Task Handle(SupplierUpdated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
