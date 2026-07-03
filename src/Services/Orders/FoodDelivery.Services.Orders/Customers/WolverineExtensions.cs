using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Orders.Customers.Features.CreatingCustomer.v1.Events.External;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using Wolverine;

namespace FoodDelivery.Services.Orders.Customers;

internal static class WolverineExtensions
{
    internal static void AddCustomerEndpoints(this WolverineOptions options)
    {
        options.ListenToPrimaryExchange<CustomerCreatedV1>();
    }
}
