using Bogus;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;

namespace ECommerce.Services.Customers.TestShared.Fakes.Customers.Events;

//https://github.com/bchavez/Bogus#using-fakert-inheritance
public sealed class FakeUserRegisteredV1 : Faker<UserRegisteredV1>
{
    public FakeUserRegisteredV1()
    {
        CustomInstantiator(f => new UserRegisteredV1(
            Guid.NewGuid(),
            f.Person.Email,
            f.Phone.PhoneNumber("(+##)##########"),
            f.Person.UserName,
            f.Person.FirstName,
            f.Person.LastName,
            new List<string> {"user"}));
    }
}
