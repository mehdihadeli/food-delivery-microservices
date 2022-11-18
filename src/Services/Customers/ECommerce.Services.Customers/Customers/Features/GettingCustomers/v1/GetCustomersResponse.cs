using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Customers.Customers.Dtos;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomers.v1;

public record GetCustomersResponse(ListResultModel<CustomerReadDto> Customers);
