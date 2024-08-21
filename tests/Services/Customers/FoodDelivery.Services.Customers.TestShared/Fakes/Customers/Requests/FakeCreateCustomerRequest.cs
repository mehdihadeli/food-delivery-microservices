using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Requests;

internal sealed class FakeCreateCustomerRequest : AutoFaker<CreateCustomerRequest>
{
    public FakeCreateCustomerRequest(string? email = null)
    {
        RuleFor(x => x.Email, f => email ?? f.Internet.Email());
    }
}
