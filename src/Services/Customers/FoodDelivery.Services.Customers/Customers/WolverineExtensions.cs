using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Customers.Customers;

public static class WolverineExtensions
{
    public static void ConfigureCustomerPublishMessagesTopology(this WolverineOptions options)
    {
        options.PublishToPrimaryExchange<CustomerCreatedV1>();
    }
}
