using BuildingBlocks.Abstractions.CQRS.Queries;

namespace ECommerce.Services.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

