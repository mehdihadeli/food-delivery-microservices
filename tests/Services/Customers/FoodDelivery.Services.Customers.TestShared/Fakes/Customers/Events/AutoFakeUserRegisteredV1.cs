using AutoBogus;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Events;

// beside of auto-generated fields data we can set rules for some fields
public sealed class AutoFakeUserRegisteredV1 : AutoFaker<UserRegisteredV1>
{
    public AutoFakeUserRegisteredV1()
    {
        RuleFor(r => r.Roles, r => new List<string> { "user" });
    }
}
