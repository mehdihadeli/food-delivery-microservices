using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

public record RestockSubscriptionDeleted(long RestockSubscriptionId) : DomainEvent;

internal class RestockSubscriptionDeletedHandler(ICommandBus commandBus, CustomersDbContext customersDbContext)
    : IDomainEventHandler<RestockSubscriptionDeleted>
{
    public async ValueTask Handle(RestockSubscriptionDeleted notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        // var isDeleted = (bool)_customersDbContext.Entry(notification.RestockSubscriptionReadModel)
        //     .Property("IsDeleted")
        //     .CurrentValue!;
        var restockSubscription = await customersDbContext
            .RestockSubscriptions.Include(restockSubscription => restockSubscription.ProductInformation)
            .FirstOrDefaultAsync(x => x.Id == notification.RestockSubscriptionId, cancellationToken);

        if (restockSubscription is null)
            return;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(
                restockSubscription.Id,
                restockSubscription.CustomerId,
                restockSubscription.Email,
                restockSubscription.ProductInformation.Id,
                restockSubscription.ProductInformation.Name,
                restockSubscription.Processed,
                restockSubscription.ProcessedTime,
                true
            ),
            cancellationToken
        );
    }
}
