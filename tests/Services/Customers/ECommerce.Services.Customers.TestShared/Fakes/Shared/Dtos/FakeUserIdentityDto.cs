using AutoBogus;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;

namespace ECommerce.Services.Customers.TestShared.Fakes.Shared.Dtos;

//https://github.com/bchavez/Bogus#the-great-c-example
//https://github.com/bchavez/Bogus#bogus-api-support
public sealed class FakeUserIdentityDto : AutoFaker<UserIdentityDto>
{
    public FakeUserIdentityDto(string? email = null)
    {
        RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
            .RuleFor(u => u.Email, (f, u) => email ?? f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.PhoneNumber, (f, u) => email ?? f.Phone.PhoneNumber("(+##)##########"));
    }
}
