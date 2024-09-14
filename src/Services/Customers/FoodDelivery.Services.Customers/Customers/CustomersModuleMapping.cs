using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Read.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Customers.Customers;

// https://mapperly.riok.app/docs/configuration/static-mappers/
[Mapper]
internal static partial class CustomersModuleMapping
{
    [MapProperty(nameof(CustomerCreated.Id), nameof(CreateCustomerRead.CustomerId))]
    [MapProperty(nameof(CustomerCreated.CreatedAt), nameof(CreateCustomerRead.Created))]
    [MapProperty(nameof(CustomerCreated.Address), nameof(CreateCustomerRead.DetailAddress))]
    internal static partial CreateCustomerRead ToCreateCustomerRead(this CustomerCreated customerCreated);

    [MapperIgnoreTarget(nameof(Models.Reads.Customer.Id))]
    [MapProperty(nameof(CreateCustomerRead.CustomerId), nameof(Models.Reads.Customer.CustomerId))]
    internal static partial Models.Reads.Customer ToCustomer(this CreateCustomerRead createCustomerRead);

    [MapProperty(nameof(Models.Reads.Customer.FullName), nameof(CustomerReadDto.Name))]
    internal static partial CustomerReadDto ToCustomerReadDto(this Models.Reads.Customer customer);

    // https://mapperly.riok.app/docs/configuration/flattening/
    [MapProperty(nameof(Models.Customer.Id), nameof(CreateCustomerRead.CustomerId))]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.Country)}",
        nameof(CreateCustomerRead.Country)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.City)}",
        nameof(CreateCustomerRead.City)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.Detail)}",
        nameof(CreateCustomerRead.DetailAddress)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Nationality)}.{nameof(Models.Customer.Nationality.Value)}",
        nameof(CreateCustomerRead.Nationality)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Email)}.{nameof(Models.Customer.Email.Value)}",
        nameof(CreateCustomerRead.Email)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.BirthDate)}.{nameof(Models.Customer.BirthDate.Value)}",
        nameof(CreateCustomerRead.BirthDate)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.PhoneNumber)}.{nameof(Models.Customer.PhoneNumber.Value)}",
        nameof(CreateCustomerRead.PhoneNumber)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.FirstName)}",
        nameof(CreateCustomerRead.FirstName)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.LastName)}",
        nameof(CreateCustomerRead.LastName)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.FullName)}",
        nameof(CreateCustomerRead.FullName)
    )]
    internal static partial CreateCustomerRead ToCreateCustomerRead(this Models.Customer customer);

    // https://mapperly.riok.app/docs/configuration/flattening/
    [MapProperty(nameof(Models.Customer.Id), nameof(UpdateCustomerRead.CustomerId))]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.City)}",
        nameof(UpdateCustomerRead.Country)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.City)}",
        nameof(UpdateCustomerRead.City)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Address)}.{nameof(Models.Customer.Address.Detail)}",
        nameof(UpdateCustomerRead.DetailAddress)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Nationality)}.{nameof(Models.Customer.Nationality.Value)}",
        nameof(UpdateCustomerRead.Nationality)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Email)}.{nameof(Models.Customer.Email.Value)}",
        nameof(UpdateCustomerRead.Email)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.BirthDate)}.{nameof(Models.Customer.BirthDate.Value)}",
        nameof(UpdateCustomerRead.BirthDate)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.PhoneNumber)}.{nameof(Models.Customer.PhoneNumber.Value)}",
        nameof(UpdateCustomerRead.PhoneNumber)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.FirstName)}",
        nameof(UpdateCustomerRead.FirstName)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.LastName)}",
        nameof(UpdateCustomerRead.LastName)
    )]
    [MapProperty(
        $"{nameof(Models.Customer.Name)}.{nameof(Models.Customer.Name.FullName)}",
        nameof(UpdateCustomerRead.FullName)
    )]
    internal static partial UpdateCustomerRead ToUpdateCustomerRead(this Models.Customer customer);

    // https://mapperly.riok.app/docs/configuration/existing-target/
    // Todo: doesn't map correctly
    internal static Models.Reads.Customer ToCustomer(this UpdateCustomerRead updateCustomerRead)
    {
        return new Customer
        {
            Created = updateCustomerRead.OccurredOn,
            Email = updateCustomerRead.Email,
            CustomerId = updateCustomerRead.CustomerId,
            IdentityId = updateCustomerRead.IdentityId,
            FirstName = updateCustomerRead.FirstName,
            LastName = updateCustomerRead.LastName,
            FullName = updateCustomerRead.FullName,
            PhoneNumber = updateCustomerRead.PhoneNumber,
            Country = updateCustomerRead.Country,
            City = updateCustomerRead.City,
            DetailAddress = updateCustomerRead.DetailAddress,
            Nationality = updateCustomerRead.Nationality,
            BirthDate = updateCustomerRead.BirthDate,
            Id = updateCustomerRead.Id
        };
    }

    [MapperIgnoreTarget(nameof(UpdateCustomerRead.Id))]
    [MapProperty(nameof(CustomerUpdated.Id), nameof(UpdateCustomerRead.CustomerId))]
    [MapProperty(nameof(CustomerUpdated.IdentityId), nameof(UpdateCustomerRead.IdentityId))]
    internal static partial UpdateCustomerRead ToUpdateCustomerRead(this CustomerUpdated customerUpdated);

    // https://mapperly.riok.app/docs/configuration/mapper/#user-implemented-property-mappings
    // https://mapperly.riok.app/docs/configuration/user-implemented-methods/
    [MapPropertyFromSource(nameof(UpdateCustomer.Id), Use = nameof(MapUpdateCustomerDefaultId))]
    [MapProperty(nameof(UpdateCustomerRequest.Address), nameof(UpdateCustomer.DetailAddress))]
    [MapProperty(nameof(UpdateCustomerRequest.BirthDate), nameof(UpdateCustomer.BirthDate))]
    [MapProperty(nameof(UpdateCustomerRequest.PhoneNumber), nameof(UpdateCustomer.PhoneNumber))]
    [MapProperty(nameof(UpdateCustomerRequest.Email), nameof(UpdateCustomer.Email))]
    internal static partial UpdateCustomer ToUpdateCustomer(this UpdateCustomerRequest updateCustomerRequest);

    private static long MapUpdateCustomerDefaultId(UpdateCustomerRequest request) => 0;

    internal static partial IQueryable<CustomerReadDto> ProjectToCustomerReadDto(
        this IQueryable<Models.Reads.Customer> queryable
    );
}
