using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Notification;
using ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.v1.Events.Domain;

namespace ECommerce.Services.Catalogs.Products;

public class ProductEventMapper : IEventMapper
{
    public ProductEventMapper()
    {

    }

    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductCreated e =>
                new Services.Shared.Catalogs.Products.Events.v1.Integration.ProductCreated(
                    e.Product.Id,
                    e.Product.Name,
                    e.Product.Category.Id,
                    e.Product.Category.Name,
                    e.Product.Stock.Available),
            ProductStockDebited e => new
                Services.Shared.Catalogs.Products.Events.v1.Integration.ProductStockDebited(
                    e.ProductId, e.NewStock.Available, e.DebitedQuantity),
            ProductStockReplenished e => new
                Services.Shared.Catalogs.Products.Events.v1.Integration.ProductStockReplenished(
                    e.ProductId, e.NewStock.Available, e.ReplenishedQuantity),
            _ => null
        };
    }

    public IDomainNotificationEvent? MapToDomainNotificationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductCreated e => new ProductCreatedNotification(e),
            _ => null
        };
    }

    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList().AsReadOnly();
    }

    public IReadOnlyList<IDomainNotificationEvent?> MapToDomainNotificationEvents(
        IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToDomainNotificationEvent).ToList().AsReadOnly();
    }
}
