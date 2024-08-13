using BuildingBlocks.Core.Events.Internal;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.V1.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent;
