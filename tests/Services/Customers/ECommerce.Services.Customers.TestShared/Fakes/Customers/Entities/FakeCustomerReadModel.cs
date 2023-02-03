using AutoBogus;
using ECommerce.Services.Customers.Customers.Models.Reads;

namespace ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;

public sealed class FakeCustomerReadModel : AutoFaker<CustomerReadModel>
{
    public FakeCustomerReadModel()
    {
        int customerId = 1;
        RuleFor(x => x.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.FullName, (f, u) => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.PhoneNumber, (f, u) => f.Phone.PhoneNumber("(+##)##########"))
            .RuleFor(u => u.City, (f, u) => f.Address.City())
            .RuleFor(u => u.Country, (f, u) => f.Address.Country())
            .RuleFor(u => u.DetailAddress, (f, u) => f.Address.FullAddress())
            .RuleFor(u => u.BirthDate, (f, u) => DateTime.Now)
            .RuleFor(u => u.CustomerId, (f, u) => customerId++);
    }
}
