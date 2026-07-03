using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierDeleted.Events.Integration.External;

[AsyncApi]
public class SupplierDeletedConsumer(
    HeaderPropagationValues headerPropagationValues,
    IMessagePersistenceService messagePersistenceService
)
{
    public Task Handle(SupplierDeletedV1 message, Envelope envelope)
    {
        return WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            messagePersistenceService,
            _ => Task.CompletedTask
        );
    }
}
