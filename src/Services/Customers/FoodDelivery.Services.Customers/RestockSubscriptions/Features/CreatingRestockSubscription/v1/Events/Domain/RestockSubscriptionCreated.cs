using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Data;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record RestockSubscriptionCreated(
    long Id,
    long ProductId,
    long CustomerId,
    string ProductName,
    string Email,
    DateTime Created,
    bool Processed
) : DomainEvent
{
    public static RestockSubscriptionCreated Of(
        long id,
        long productId,
        long customerId,
        string productName,
        string email,
        DateTime created,
        bool processed
    )
    {
        id.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();
        customerId.NotBeNegativeOrZero();
        productName.NotBeNullOrWhiteSpace();
        email.NotBeNullOrWhiteSpace();
        created.NotBeEmpty();

        return new RestockSubscriptionCreated(id, productId, customerId, productName, email, created, processed);
    }

    public CreateMongoRestockSubscriptionReadModels ToCreateMongoRestockSubscriptionReadModels(
        long customerId,
        string customerName
    )
    {
        return new CreateMongoRestockSubscriptionReadModels(
            Id,
            customerId,
            customerName,
            ProductId,
            ProductName,
            Email,
            Processed
        );
    }
}

public class RestockSubscriptionCreatedHandler(ICommandBus commandBus, CustomersDbContext customersDbContext)
    : IDomainEventHandler<RestockSubscriptionCreated>
{
    public async ValueTask Handle(RestockSubscriptionCreated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var customer = await customersDbContext.FindCustomerByIdAsync(CustomerId.Of(notification.CustomerId));

        if (customer is null)
        {
            throw new CustomerNotFoundException(notification.CustomerId);
        }

        var mongoReadCommand = notification.ToCreateMongoRestockSubscriptionReadModels(
            customer!.Id,
            customer.Name.FullName
        );

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        await commandBus.ScheduleAsync([mongoReadCommand], cancellationToken);
    }
}
