using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

public static class RestockSubscriptionManualMappings
{
    internal static RestockSubscriptionReadModel ToRestockSubscription(this RestockSubscription restockSubscription)
    {
        return new RestockSubscriptionReadModel
        {
            RestockSubscriptionId = restockSubscription.Id,
            CustomerId = restockSubscription.CustomerId,
            ProductId = restockSubscription.ProductInformation.Id,
            ProductName = restockSubscription.ProductInformation.Name,
            Created = restockSubscription.Created,
            Processed = restockSubscription.Processed,
            ProcessedTime = restockSubscription.ProcessedTime,
            Email = restockSubscription.Email,
            CreatedBy = restockSubscription.CreatedBy,
        };
    }

    internal static RestockSubscriptionDto ToRestockSubscriptionDto(this RestockSubscription restockSubscription)
    {
        return new RestockSubscriptionDto(
            restockSubscription.Id,
            restockSubscription.CustomerId,
            null,
            restockSubscription.Email,
            restockSubscription.ProductInformation.Id,
            restockSubscription.ProductInformation.Name,
            restockSubscription.Created,
            restockSubscription.Processed,
            restockSubscription.ProcessedTime
        );
    }
}
