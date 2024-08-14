using BuildingBlocks.Abstractions.Queries;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingAvailableStockById.v1;

public record GetAvailableStockById(long ProductId) : IQuery<int>;
