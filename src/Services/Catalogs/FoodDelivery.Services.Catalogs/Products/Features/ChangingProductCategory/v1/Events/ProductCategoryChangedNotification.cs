using BuildingBlocks.Core.Domain.Events.Internal;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.V1.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent
{
    public override bool Equals(object obj)
    {
        return Equals(obj as ProductCategoryChangedNotification);
    }
}
