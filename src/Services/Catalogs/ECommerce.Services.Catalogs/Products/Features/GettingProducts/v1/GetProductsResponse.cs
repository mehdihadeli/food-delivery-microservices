using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Catalogs.Products.Dtos;
using ECommerce.Services.Catalogs.Products.Dtos.v1;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProducts.v1;

public record GetProductsResponse(ListResultModel<ProductDto> Products);
