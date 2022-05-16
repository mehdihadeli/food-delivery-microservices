using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ProductCategoryChanged(CategoryId CategoryId, ProductId ProductId) :
    DomainEvent;
