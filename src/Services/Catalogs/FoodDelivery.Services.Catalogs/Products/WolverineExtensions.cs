using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Catalogs.Products;

public static class WolverineExtensions
{
    public static void ConfigureProductPublishMessagesTopology(this WolverineOptions options)
    {
        options.PublishToPrimaryExchange<ProductCreatedV1>();
        options.PublishToPrimaryExchange<ProductStockDebitedV1>();
        options.PublishToPrimaryExchange<ProductStockReplenishedV1>();
    }
}
