using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert
// `Faker` has a problem with non-default constructor but `AutoFaker` works also with none-default constructor
// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one, so it is better we use Faker<>
internal sealed class FakeCreateCustomer : AutoFaker<CreateCustomer>
{
    public FakeCreateCustomer(string? email = null)
    {
        long id = 1;
        RuleFor(x => x.Email, f => email ?? f.Internet.Email());
        RuleFor(x => x.Id, f => id++);
    }
}
