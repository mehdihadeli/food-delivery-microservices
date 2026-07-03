using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Customers.Products;

public static class WolverineExtensions
{
    public static void ConfigureProductsConsumeMessagesTopology(this WolverineOptions options)
    {
        options.ListenToPrimaryExchange<ProductStockReplenishedV1>();
        options.ListenToPrimaryExchange<ProductCreatedV1>();
    }
}
