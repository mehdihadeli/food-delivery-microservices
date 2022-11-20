using AutoMapper;
using BuildingBlocks.Core.Domain.ValueObjects;
using ECommerce.Services.Customers.Customers.Dtos;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using ECommerce.Services.Customers.Customers.Features;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Customers.ValueObjects;

namespace ECommerce.Services.Customers.Customers;

public class CustomersMapping : Profile
{
    public CustomersMapping()
    {
        CreateMap<CustomerReadModel, CustomerReadDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId))
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Country))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.City))
            .ForMember(x => x.DetailAddress, opt => opt.MapFrom(x => x.DetailAddress))
            .ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
            .ForMember(x => x.BirthDate, opt => opt.MapFrom(x => x.BirthDate))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber));

        CreateMap<Customer, CreateMongoCustomerReadModels>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Created, opt => opt.MapFrom(x => x.Created))
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.City))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.City))
            .ForMember(
                x => x.DetailAddress,
                opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.Detail))
            .ForMember(
                x => x.Nationality,
                opt => opt.MapFrom(x => x.Nationality == Nationality.Null ? null : x.Nationality!.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(
                x => x.BirthDate,
                opt => opt.MapFrom(x => x.BirthDate == BirthDate.Null ? null : x.BirthDate!.Value as DateTime?))
            .ForMember(
                x => x.PhoneNumber,
                opt => opt.MapFrom(x => x.PhoneNumber == PhoneNumber.Null ? "" : x.PhoneNumber!.Value))
            .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Name.FirstName))
            .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Name.LastName))
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name.FullName));

        CreateMap<CreateMongoCustomerReadModels, CustomerReadModel>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId));

        CreateMap<Customer, UpdateMongoCustomerReadsModel>()
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.City))
            .ForMember(x => x.City, opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.City))
            .ForMember(
                x => x.DetailAddress,
                opt => opt.MapFrom(x => x.Address == Address.Null ? "" : x.Address!.Detail))
            .ForMember(
                x => x.Nationality,
                opt => opt.MapFrom(x => x.Nationality == Nationality.Null ? null : x.Nationality!.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(
                x => x.BirthDate,
                opt => opt.MapFrom(x => x.BirthDate == BirthDate.Null ? null : x.BirthDate!.Value as DateTime?))
            .ForMember(
                x => x.PhoneNumber,
                opt => opt.MapFrom(x => x.PhoneNumber == PhoneNumber.Null ? "" : x.PhoneNumber!.Value))
            .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Name.FirstName))
            .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Name.LastName))
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name.FullName));


        CreateMap<UpdateMongoCustomerReadsModel, CustomerReadModel>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId));
    }
}
