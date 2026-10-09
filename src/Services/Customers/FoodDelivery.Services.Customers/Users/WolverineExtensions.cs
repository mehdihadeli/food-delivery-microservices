using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Squirrel.Integration.Wolverine;
using Wolverine;

namespace FoodDelivery.Services.Customers.Users;

public static class WolverineExtensions
{
    public static void ConfigureUsersConsumeMessagesTopology(this WolverineOptions options)
    {
        options.ListenToPrimaryExchange<UserRegisteredV1>();
    }
}
