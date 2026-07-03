using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Identity.Users;

public static class WolverineExtensions
{
    public static void ConfigureUserPublishMessagesTopology(this WolverineOptions options)
    {
        options.PublishToPrimaryExchange<UserRegisteredV1>();
        options.PublishToPrimaryExchange<UserStateUpdatedV1>();
    }
}
