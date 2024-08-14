using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record RestockNotificationProcessed(long Id, DateTime ProcessedTime) : DomainEvent;

internal class RestockNotificationProcessedHandler(ICommandBus commandBus, CustomersDbContext customersDbContext)
    : IDomainEventHandler<RestockNotificationProcessed>
{
    public async Task Handle(RestockNotificationProcessed notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var restockSubscription = await customersDbContext
            .RestockSubscriptions.Include(restockSubscription => restockSubscription.ProductInformation)
            .FirstOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

        if (restockSubscription is null)
            return;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(
                notification.Id,
                restockSubscription.CustomerId,
                restockSubscription.Email,
                restockSubscription.ProductInformation.Id,
                restockSubscription.ProductInformation.Name,
                restockSubscription.Processed,
                notification.ProcessedTime
            ),
            cancellationToken
        );
    }
}
