using BuildingBlocks.Abstractions.CQRS.Queries;

namespace ECommerce.Services.Catalogs.Products.Features.GettingAvailableStockById.v1;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

