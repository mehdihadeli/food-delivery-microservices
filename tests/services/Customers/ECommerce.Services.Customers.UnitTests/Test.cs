using AutoBogus;
using Bogus;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using Tests.Shared.Mocks.Customers.Events;

namespace ECommerce.Services.Customers.UnitTests;

public class Test
{
    [Fact]
    public void Test1()
    {
        var s = new Faker<UserRegisteredV1>().Generate(2);
    }
}
