using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent;
