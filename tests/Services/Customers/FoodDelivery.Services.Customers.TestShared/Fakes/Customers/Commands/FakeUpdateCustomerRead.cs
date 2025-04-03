using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Internal.Mongo;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert
// `Faker` has a problem with non-default constructor but `AutoFaker` works also with none-default constructor
// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one, so it is better we use Faker<>
internal sealed class FakeUpdateCustomerRead : AutoFaker<UpdateCustomerRead>
{
    public FakeUpdateCustomerRead(Guid id, long customerId, Guid identityId)
    {
        RuleFor(x => x.CustomerId, f => customerId)
            .RuleFor(x => x.Id, (f, u) => id)
            .RuleFor(x => x.IdentityId, (f, u) => identityId)
            .RuleFor(x => x.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.FullName, (f, u) => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.PhoneNumber, (f, u) => f.Phone.PhoneNumber("(+##)##########"))
            .RuleFor(u => u.City, (f, u) => f.Address.City())
            .RuleFor(u => u.Country, (f, u) => f.Address.Country())
            .RuleFor(u => u.DetailAddress, (f, u) => f.Address.FullAddress())
            .RuleFor(u => u.BirthDate, (f, u) => DateTime.Now);
    }
}
