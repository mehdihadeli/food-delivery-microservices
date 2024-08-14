using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Events;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;
using MassTransit;

namespace FoodDelivery.Services.Customers.Products.Features.ReplenishingProductStock.v1.Events.Integration.External;

public class ProductStockReplenishedConsumer(ICommandBus commandBus, ILogger<ProductStockReplenishedConsumer> logger)
    : IConsumer<EventEnvelope<ProductStockReplenishedV1>>
{
    // If this handler is called successfully, it will send a ACK to rabbitmq for removing message from the queue and if we have an exception it send an NACK to rabbitmq
    // and with NACK we can retry the message with re-queueing this message to the broker
    public async Task Consume(ConsumeContext<EventEnvelope<ProductStockReplenishedV1>> context)
    {
        var productStockReplenished = context.Message.Data;

        await commandBus.SendAsync(
            ProcessRestockNotification.Of(productStockReplenished.ProductId, productStockReplenished.NewStock)
        );

        logger.LogInformation(
            "Sending restock notification command for product {ProductId}",
            productStockReplenished.ProductId
        );
    }
}
