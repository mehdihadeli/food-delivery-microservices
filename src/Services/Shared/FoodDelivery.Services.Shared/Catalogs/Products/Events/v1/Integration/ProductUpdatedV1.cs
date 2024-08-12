using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;

public record ProductUpdatedV1(long Id, string Name, long CategoryId, string CategoryName, int Stock) : IntegrationEvent
{
    /// <summary>
    /// ProductUpdatedV1 with in-line validation.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="categoryId"></param>
    /// <param name="categoryName"></param>
    /// <param name="stock"></param>
    /// <returns></returns>
    public static ProductUpdatedV1 Of(long id, string? name, long categoryId, string? categoryName, int stock)
    {
        id.NotBeNegativeOrZero();
        name.NotBeNullOrWhiteSpace();
        categoryId.NotBeNegativeOrZero();
        categoryName.NotBeNullOrWhiteSpace();
        stock.NotBeNegativeOrZero();

        return new ProductUpdatedV1(id, name, categoryId, categoryName, stock);
    }
}
