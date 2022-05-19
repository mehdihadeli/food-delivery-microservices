using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.External;

public record SupplierCreated(long Id, string Name) : IntegrationEvent;


public class SupplierCreatedConsumer : IEventHandler<SupplierCreated>
{
    public Task Handle(SupplierCreated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
