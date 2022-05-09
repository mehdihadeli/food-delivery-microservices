using BuildingBlocks.Abstractions.CQRS.Query;

namespace Store.Services.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

