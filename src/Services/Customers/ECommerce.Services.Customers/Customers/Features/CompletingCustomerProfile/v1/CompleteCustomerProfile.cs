using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Dtos.v1;

namespace ECommerce.Services.Customers.Customers.Features.CompletingCustomerProfile.v1;

public record CompleteCustomerProfile(string Email, AddressDto Address, DateTime BirthDate, string Nationality) :
    ITxCreateCommand<CompleteCustomerProfileResponse>;
