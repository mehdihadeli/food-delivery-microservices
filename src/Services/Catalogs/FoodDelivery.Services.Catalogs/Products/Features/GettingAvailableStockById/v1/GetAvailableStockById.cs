using BuildingBlocks.Abstractions.Queries;

namespace FoodDelivery.Services.Catalogs.Products.Features.GettingAvailableStockById.V1;

public record GetAvailableStockById(long ProductId) : IQuery<int>;
