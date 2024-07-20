using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.Read.Mongo;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

internal sealed class FakeUpdateCustomerRead : AutoFaker<UpdateCustomerRead>
{
    public FakeUpdateCustomerRead()
    {
        long id = 1;
        RuleFor(x => x.CustomerId, f => id++);
    }
}
