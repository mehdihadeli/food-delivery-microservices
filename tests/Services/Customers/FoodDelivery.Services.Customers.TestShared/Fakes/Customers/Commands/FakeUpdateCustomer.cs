using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// Auto Faker works only for public constructors
// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert
// `Faker` has a problem with non-default constructor but `AutoFaker` works also with none-default constructor
// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one, so it is better we use Faker<>
internal sealed class FakeUpdateCustomer : AutoFaker<UpdateCustomer>
{
    public FakeUpdateCustomer(long customerId)
    {
        RuleFor(x => x.Email, f => f.Internet.Email());
        RuleFor(x => x.Id, customerId);
        RuleFor(x => x.FirstName, f => f.Name.FirstName());
        RuleFor(x => x.LastName, f => f.Name.LastName());
        RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber("(+##)##########"));
        RuleFor(x => x.BirthDate, DateTime.Now.AddYears(-20));
        RuleFor(x => x.Nationality, "IR");
    }
}
