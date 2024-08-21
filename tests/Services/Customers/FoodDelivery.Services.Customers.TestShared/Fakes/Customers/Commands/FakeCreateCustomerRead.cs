using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

internal sealed class FakeCreateCustomerRead : AutoFaker<CreateCustomerRead>
{
    public FakeCreateCustomerRead()
    {
        long id = 1;
        RuleFor(x => x.CustomerId, f => id++);
    }
}
