using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;
using ICommandBus = BuildingBlocks.Abstractions.Commands.ICommandBus;

namespace FoodDelivery.Services.Customers.Products.Features.ReplenishingProductStock.v1.Events.Integration.External;

[AsyncApi]
public class ProductStockReplenishedConsumer(
    ICommandBus commandBus,
    ILogger<ProductStockReplenishedConsumer> logger,
    HeaderPropagationValues headerPropagationValues
)
{
    // If this handler is called successfully, it will send a ACK to rabbitmq for removing message from the queue and if we have an exception it send an NACK to rabbitmq
    // and with NACK we can retry the message with re-queueing this message to the broker
    public async Task Handle(ProductStockReplenishedV1 message, Envelope envelope)
    {
        await WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            async productStockReplenished =>
            {
                await commandBus.SendAsync(
                    ProcessRestockNotification.Of(productStockReplenished.ProductId, productStockReplenished.NewStock)
                );

                logger.LogInformation(
                    "Sending restock notification command for product {ProductId}",
                    productStockReplenished.ProductId
                );
            },
            logger
        );
    }
}
