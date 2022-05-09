using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Categories;
using Store.Services.Catalogs.Products.ValueObjects;

namespace Store.Services.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent;
