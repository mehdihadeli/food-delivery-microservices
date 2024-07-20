using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Events.Domain;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1.Events.Domain;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
public class RestockSubscription : Aggregate<RestockSubscriptionId>, IHaveSoftDelete
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    public RestockSubscription() { }

    public CustomerId CustomerId { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public ProductInformation ProductInformation { get; private set; } = default!;
    public bool Processed { get; private set; }
    public DateTime? ProcessedTime { get; private set; }

    public static RestockSubscription Create(
        RestockSubscriptionId id,
        CustomerId customerId,
        ProductInformation productInformation,
        Email email
    )
    {
        id.NotBeNull();
        customerId.NotBeNull();
        productInformation.NotBeNull();

        var restockSubscription = new RestockSubscription
        {
            Id = id,
            CustomerId = customerId,
            ProductInformation = productInformation
        };

        restockSubscription.ChangeEmail(email);

        restockSubscription.AddDomainEvents(
            RestockSubscriptionCreated.Of(
                id,
                productInformation.Id,
                customerId,
                productInformation.Name,
                email,
                restockSubscription.Created,
                false
            )
        );

        return restockSubscription;
    }

    public void ChangeEmail(Email email)
    {
        email.NotBeNull();
        Email = email;
    }

    public void Delete()
    {
        AddDomainEvents(new RestockSubscriptionDeleted(Id));
    }

    public void MarkAsProcessed(DateTime processedTime)
    {
        Processed = true;
        ProcessedTime = processedTime;

        AddDomainEvents(new RestockNotificationProcessed(Id, processedTime));
    }
}
