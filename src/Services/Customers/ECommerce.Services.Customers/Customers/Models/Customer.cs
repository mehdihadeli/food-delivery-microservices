using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.ValueObjects;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using ECommerce.Services.Customers.Customers.ValueObjects;

namespace ECommerce.Services.Customers.Customers.Models;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
public class Customer : Aggregate<CustomerId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Customer()
    {
    }

    public Guid IdentityId { get; private set; }
    public Email Email { get; private set; } = default!;
    public CustomerName Name { get; private set; } = default!;
    public Address? Address { get; private set; }
    public Nationality? Nationality { get; private set; }
    public BirthDate? BirthDate { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = default!;

    public static Customer Create(
        CustomerId id,
        Email email,
        PhoneNumber phoneNumber,
        CustomerName name,
        Guid identityId)
    {
        var customer = new Customer
        {
            Id = Guard.Against.Null(id),
            Email = Guard.Against.Null(email),
            PhoneNumber = Guard.Against.Null(phoneNumber),
            Name = Guard.Against.Null(name),
            IdentityId = Guard.Against.NullOrEmpty(identityId),
        };

        customer.AddDomainEvents(new CustomerCreated(customer));

        return customer;
    }
}
