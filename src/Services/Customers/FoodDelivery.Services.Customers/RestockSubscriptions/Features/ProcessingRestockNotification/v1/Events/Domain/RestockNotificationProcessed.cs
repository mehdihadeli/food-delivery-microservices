using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.V1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record RestockNotificationProcessed(long Id, DateTime ProcessedTime) : DomainEvent
{
    public override bool Equals(object obj)
    {
        return Equals(obj as RestockNotificationProcessed);
    }
}

internal class RestockNotificationProcessedHandler : IDomainEventHandler<RestockNotificationProcessed>
{
    private readonly ICommandBus _commandProcessor;
    private readonly CustomersDbContext _customersDbContext;

    public RestockNotificationProcessedHandler(ICommandBus commandProcessor, CustomersDbContext customersDbContext)
    {
        _commandProcessor = commandProcessor;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockNotificationProcessed notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var restockSubscription = await _customersDbContext.RestockSubscriptions.FirstOrDefaultAsync(
            x => x.Id == notification.Id,
            cancellationToken
        );

        if (restockSubscription is null)
            return;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandProcessor.SendAsync(
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
