using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Customers.Customers.Events.V1.Integration;

public record CustomerCreatedV1(long CustomerId) : IntegrationEvent
{
    /// <summary>
    /// CustomerCreatedV1 with in-line validation.
    /// </summary>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public static CustomerCreatedV1 Of(long customerId) => new CustomerCreatedV1(customerId.NotBeNegativeOrZero());
}
