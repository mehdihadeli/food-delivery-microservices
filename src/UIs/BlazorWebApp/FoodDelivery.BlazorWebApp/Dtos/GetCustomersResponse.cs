using BuildingBlocks.Core.Paging;

namespace FoodDelivery.BlazorWebApp.Dtos;

public record GetCustomersResponse(PageList<CustomersDto> CustomersPageList);
