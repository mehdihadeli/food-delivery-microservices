using Bogus;
using BuildingBlocks.Core.Domain.ValueObjects;
using FoodDelivery.Services.Customers.Customers.Models;
using FoodDelivery.Services.Customers.Customers.ValueObjects;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// https://github.com/nickdodd79/AutoBogus/blob/8c182f937b65719e7b59bc479546caf3a97fc135/src/AutoBogus/AutoMember.cs#L28
// https://github.com/nickdodd79/AutoBogus/blob/8c182f937b65719e7b59bc479546caf3a97fc135/src/AutoBogus/AutoBinder.cs#L92

// Auto Faker works only for public constructors
// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert
// `Faker` has a problem with non-default constructor but `AutoFaker` works also with none-default constructor
// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one, so it is better we use Faker<>
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
                Guid.NewGuid(),
                Address.Of(
                    f.Address.Country(),
                    f.Address.City(),
                    "detail address sample",
                    PostalCode.Of(f.Address.ZipCode())
                ),
                BirthDate.Of(DateTime.Now.AddYears(-20)),
                Nationality.Of("US")
            );
        });
    }

    public FakeCustomer(long id, Guid identityId)
    {
        // we should not instantiate customer aggregate manually because it is possible we break aggregate invariant in creating a customer, and it is better we
        // create a customer with its factory method
        CustomInstantiator(f =>
        {
            var firstName = f.Name.FirstName();
            var lastName = f.Name.LastName();

            var customer = Customer.Create(
                CustomerId.Of(id),
                Email.Of(f.Internet.Email(firstName, lastName)),
                PhoneNumber.Of(f.Phone.PhoneNumber("(+##)##########")),
                CustomerName.Of(firstName, lastName),
                identityId,
                Address.Of(
                    f.Address.Country(),
                    f.Address.City(),
                    f.Address.FullAddress(),
                    PostalCode.Of(f.Address.ZipCode())
                ),
                BirthDate.Of(DateTime.Now.AddYears(-20)),
                Nationality.Of("IR")
            );

            return customer;
        });
    }
}
