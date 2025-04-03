using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;

namespace FoodDelivery.Services.Customers.Customers;

public static class CustomersManualMappings
{
    internal static CreateCustomerRead ToCreateCustomerRead(this Models.Customer customer)
    {
        return new CreateCustomerRead(
            customer.Id,
            customer.IdentityId,
            customer.Email,
            customer.Name.FirstName,
            customer.Name.LastName,
            customer.PhoneNumber,
            customer.BirthDate,
            customer.Address?.Country,
            customer.Address?.City,
            customer.Address?.Detail,
            customer.Nationality
        );
    }

    internal static CustomerReadModel ToCustomerReadModel(this CreateCustomerRead createCustomerRead)
    {
        return new CustomerReadModel
        {
            PhoneNumber = createCustomerRead.PhoneNumber,
            Email = createCustomerRead.Email,
            CustomerId = createCustomerRead.CustomerId,
            IdentityId = createCustomerRead.IdentityId,
            FirstName = createCustomerRead.FirstName,
            LastName = createCustomerRead.LastName,
            FullName = createCustomerRead.FullName,
            BirthDate = createCustomerRead.BirthDate,
            Nationality = createCustomerRead.Nationality,
            City = createCustomerRead.City,
            DetailAddress = createCustomerRead.DetailAddress,
            Country = createCustomerRead.Country,
            Created = createCustomerRead.OccurredOn,
        };
    }

    internal static CustomerReadDto ToCustomerReadDto(this CustomerReadModel customerReadModel)
    {
        return new CustomerReadDto(
            customerReadModel.Id,
            customerReadModel.CustomerId,
            customerReadModel.IdentityId,
            customerReadModel.Email,
            customerReadModel.FullName,
            customerReadModel.Created,
            customerReadModel.Country,
            customerReadModel.City,
            customerReadModel.DetailAddress,
            customerReadModel.Nationality,
            customerReadModel.BirthDate,
            customerReadModel.PhoneNumber
        );
    }

    public static IQueryable<CustomerReadDto> ToCustomersReadDto(this IQueryable<CustomerReadModel> products)
    {
        return products.Select(customerReadModel => new CustomerReadDto(
            customerReadModel.Id,
            customerReadModel.CustomerId,
            customerReadModel.IdentityId,
            customerReadModel.Email,
            customerReadModel.FullName,
            customerReadModel.Created,
            customerReadModel.Country,
            customerReadModel.City,
            customerReadModel.DetailAddress,
            customerReadModel.Nationality,
            customerReadModel.BirthDate,
            customerReadModel.PhoneNumber
        ));
    }
}
