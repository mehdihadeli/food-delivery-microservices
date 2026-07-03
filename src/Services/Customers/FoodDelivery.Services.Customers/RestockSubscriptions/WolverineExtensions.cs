using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

public static class WolverineExtensions
{
    public static void ConfigureRestockSubscriptionPublishMessagesTopology(this WolverineOptions options)
    {
        options.PublishToPrimaryExchange<RestockSubscriptionCreatedV1>();
    }
}
