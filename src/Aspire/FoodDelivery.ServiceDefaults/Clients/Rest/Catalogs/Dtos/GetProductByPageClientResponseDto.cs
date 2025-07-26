using BuildingBlocks.Core.Paging;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs.Dtos;

public record GetProductByPageClientResponseDto(PageList<ProductClientDto> Products);
