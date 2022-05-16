using BuildingBlocks.Abstractions.CQRS.Query;

namespace ECommerce.Services.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

