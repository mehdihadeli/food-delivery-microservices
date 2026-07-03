using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Customers.Users;

public static class WolverineExtensions
{
    public static void ConfigureUsersConsumeMessagesTopology(this WolverineOptions options)
    {
        options.ListenToPrimaryExchange<UserRegisteredV1>();
    }
}
