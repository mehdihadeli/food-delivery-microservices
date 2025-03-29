using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Events.Internal;
using FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;

namespace FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Notification;

public record ProductCreatedNotification(ProductCreated DomainEvent)
    : DomainNotificationEvent<ProductCreated>(DomainEvent);

public class ProductCreatedNotificationHandler(IExternalEventBus bus)
    : IDomainNotificationEventHandler<ProductCreatedNotification, ProductCreated>
{
    private readonly IExternalEventBus _bus = bus;

    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        // We can publish integration event to bus here
        // await _bus.PublishAsync(
        //     new FoodDelivery.Services.Shared.Catalogs.Products.Events.Integration.ProductCreatedV1(
        //         notification.InternalCommandId,
        //         notification.Name,
        //         notification.Stock,
        //         notification.CategoryName ?? "",
        //         notification.Stock),
        //     null,
        //     cancellationToken);
        return ValueTask.CompletedTask;
    }
}
