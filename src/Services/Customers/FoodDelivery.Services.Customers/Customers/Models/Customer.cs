using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.ValueObjects;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.ValueObjects;

namespace FoodDelivery.Services.Customers.Customers.Models;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
public class Customer : Aggregate<CustomerId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Customer() { }

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
        Guid identityId,
        Address? address = null,
        BirthDate? birthDate = null,
        Nationality? nationality = null
    )
    {
        // input validation will do in the `command` and our `value objects` before arriving to entity and makes or domain cleaner, here we just do business validation
        var customer = new Customer
        {
            Id = id,
            Email = email,
            PhoneNumber = phoneNumber,
            Name = name,
            IdentityId = identityId,
            BirthDate = birthDate,
            Address = address,
            Nationality = nationality
        };

        var (firstName, lastName) = name;

        customer.AddDomainEvents(
            CustomerCreated.Of(
                id,
                firstName,
                lastName,
                email,
                phoneNumber,
                identityId,
                DateTime.Now,
                address?.Country,
                address?.City,
                address?.Detail,
                birthDate!,
                nationality!
            )
        );

        return customer;
    }

    public void Update(
        Email email,
        PhoneNumber phoneNumber,
        CustomerName name,
        Address? address = null,
        BirthDate? birthDate = null,
        Nationality? nationality = null
    )
    {
        Email = email;
        PhoneNumber = phoneNumber;
        Name = name;

        if (address is { })
        {
            Address = address;
        }

        if (birthDate is { })
        {
            BirthDate = birthDate;
        }

        if (nationality is { })
        {
            Nationality = nationality;
        }

        var (firstName, lastName) = name;

        AddDomainEvents(
            CustomerUpdated.Of(
                Id,
                firstName,
                lastName,
                email,
                phoneNumber,
                IdentityId,
                DateTime.Now,
                birthDate!,
                address?.Country,
                address?.City,
                address?.Detail,
                nationality!
            )
        );
    }

    public void Deconstruct(
        out long id,
        out Guid identityId,
        out string email,
        out string firstName,
        out string lastName,
        out string phoneNumber,
        out string? country,
        out string? city,
        out string? detailedAddress,
        out string? nationality,
        out DateTime? birthDate
    ) =>
        (
            id,
            identityId,
            email,
            firstName,
            lastName,
            phoneNumber,
            country,
            city,
            detailedAddress,
            nationality,
            birthDate
        ) = (
            Id,
            IdentityId,
            Email,
            Name.FirstName,
            Name.LastName,
            PhoneNumber,
            Address?.Country,
            Address?.City,
            Address?.Detail,
            Nationality!,
            BirthDate!
        );
}
