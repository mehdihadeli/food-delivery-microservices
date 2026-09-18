using CloudNativeKit.Core.Paging;

namespace FoodDelivery.BlazorWebApp.Dtos;

public record GetProductsResponse(PageList<ProductDto> ProductsPageList);
