using BuildingBlocks.Core.Paging;

namespace FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Dtos;

public record GetProductByPageClientResponseDto(PageList<ProductClientDto> Products);
