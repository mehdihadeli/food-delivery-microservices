using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

[AsyncApi]
public class SupplierUpdatedConsumer(
    HeaderPropagationValues headerPropagationValues,
    IMessagePersistenceService messagePersistenceService
)
{
    public Task Handle(SupplierUpdatedV1 message, Envelope envelope)
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
