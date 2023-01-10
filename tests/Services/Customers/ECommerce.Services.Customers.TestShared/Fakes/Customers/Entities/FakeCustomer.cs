using Bogus;
using BuildingBlocks.Core.Domain.ValueObjects;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.Customers.ValueObjects;

namespace ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// https://github.com/nickdodd79/AutoBogus/blob/8c182f937b65719e7b59bc479546caf3a97fc135/src/AutoBogus/AutoMember.cs#L28
// https://github.com/nickdodd79/AutoBogus/blob/8c182f937b65719e7b59bc479546caf3a97fc135/src/AutoBogus/AutoBinder.cs#L92

// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one
public sealed class FakeCustomer : Faker<Customer>
{
    public FakeCustomer()
    {
        long id = 1;

        // we should not instantiate customer aggregate manually because it is possible we break aggregate invariant in creating a customer, and it is better we
        // create a customer with its factory method
        CustomInstantiator(f =>
        {
            var firstName = f.Name.FirstName();
            var lastName = f.Name.LastName();

            return Customer.Create(
                CustomerId.Of(id++),
                Email.Of(f.Internet.Email(firstName, lastName)),
                PhoneNumber.Of(f.Phone.PhoneNumber("(+##)##########")),
                CustomerName.Of(firstName, lastName),
                Guid.NewGuid());
        });
    }
}
