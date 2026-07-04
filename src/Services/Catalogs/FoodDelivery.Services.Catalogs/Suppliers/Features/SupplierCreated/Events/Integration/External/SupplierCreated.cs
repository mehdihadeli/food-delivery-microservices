using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Suppliers.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;

namespace FoodDelivery.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.Integration.External;

[AsyncApi]
public class SupplierCreatedConsumer(HeaderPropagationValues headerPropagationValues)
{
    public Task Handle(SupplierCreatedV1 message, Envelope envelope)
    {
        return WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            _ => Task.CompletedTask
        );
    }
}
