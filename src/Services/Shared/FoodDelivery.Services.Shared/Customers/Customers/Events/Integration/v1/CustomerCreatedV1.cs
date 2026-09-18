using CloudNativeKit.Core.Extensions;
using CloudNativeKit.Core.Messages;

namespace FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;

public record CustomerCreatedV1(long CustomerId) : IntegrationEvent
{
    /// <summary>
    /// CustomerCreatedV1 with in-line validation.
    /// </summary>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public static CustomerCreatedV1 Of(long customerId) => new CustomerCreatedV1(customerId.NotBeNegativeOrZero());
}
