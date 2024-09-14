using AutoBogus;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Read.Mongo;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;

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
