using AutoMapper;
using BuildingBlocks.Core.Domain.ValueObjects;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Read.Mongo;
using Riok.Mapperly.Abstractions;
using Customer = FoodDelivery.Services.Customers.Customers.Models.Reads.Customer;

namespace FoodDelivery.Services.Customers.Customers;

public class CustomersMapping : Profile
{
    public CustomersMapping()
    {
        CreateMap<Customer, CustomerReadDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId))
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.FullName))
            .ForMember(x => x.CreatedAt, opt => opt.MapFrom(x => x.Created))
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Country))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.City))
            .ForMember(x => x.DetailAddress, opt => opt.MapFrom(x => x.DetailAddress))
            .ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
            .ForMember(x => x.BirthDate, opt => opt.MapFrom(x => x.BirthDate))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber));

        CreateMap<Models.Customer, CreateCustomerRead>()
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Created, opt => opt.MapFrom(x => x.Created))
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.City))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.City))
            .ForMember(
                x => x.DetailAddress,
                opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.Detail)
            )
            .ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality == null ? null : x.Nationality!.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(
                x => x.BirthDate,
                opt => opt.MapFrom(x => x.BirthDate == null ? null : x.BirthDate!.Value as DateTime?)
            )
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber == null ? "" : x.PhoneNumber!.Value))
            .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Name.FirstName))
            .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Name.LastName))
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name.FullName))
            .ForMember(x => x.InternalCommandId, opt => opt.Ignore())
            .ForMember(x => x.OccurredOn, opt => opt.MapFrom(x => x.Created));

        CreateMap<Models.Customer, UpdateCustomerRead>()
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.City))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.City))
            .ForMember(
                x => x.DetailAddress,
                opt => opt.MapFrom(x => x.Address == Address.Empty ? "" : x.Address!.Detail)
            )
            .ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality == null ? null : x.Nationality!.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(
                x => x.BirthDate,
                opt => opt.MapFrom(x => x.BirthDate == null ? null : x.BirthDate!.Value as DateTime?)
            )
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber == null ? "" : x.PhoneNumber!.Value))
            .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Name.FirstName))
            .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Name.LastName))
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name.FullName))
            .ForMember(x => x.InternalCommandId, opt => opt.Ignore())
            .ForMember(x => x.OccurredOn, opt => opt.MapFrom(x => x.Created));

        CreateMap<UpdateCustomerRead, Customer>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId))
            .ForMember(x => x.Created, opt => opt.MapFrom(x => x.OccurredOn));

        CreateMap<CustomerCreated, CreateCustomerRead>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.DetailAddress, opt => opt.MapFrom(src => src.Address));

        CreateMap<CustomerUpdated, UpdateCustomerRead>();

        CreateMap<UpdateCustomerRequest, UpdateCustomer>()
            .ConstructUsing(req =>
                UpdateCustomer.Of(
                    0,
                    req.FirstName,
                    req.LastName,
                    req.Email,
                    req.PhoneNumber,
                    req.BirthDate,
                    req.Address,
                    req.Nationality
                )
            );
    }
}

// https://mapperly.riok.app/docs/configuration/static-mappers/
[Mapper]
public static partial class CustomerCreatedMapper
{
    [MapProperty(nameof(CustomerCreated.Id), nameof(CreateCustomerRead.CustomerId))]
    [MapProperty(nameof(CustomerCreated.CreatedAt), nameof(CreateCustomerRead.Created))]
    [MapProperty(nameof(CustomerCreated.Address), nameof(CreateCustomerRead.DetailAddress))]
    public static partial CreateCustomerRead ToCreateCustomerRead(this CustomerCreated customerCreated);
}

[Mapper]
public static partial class CreateCustomerReadMapper
{
    [MapperIgnoreTarget(nameof(Customer.Id))]
    [MapProperty(nameof(CreateCustomerRead.CustomerId), nameof(Customer.CustomerId))]
    public static partial Customer ToCustomer(this CreateCustomerRead createCustomerRead);
}
