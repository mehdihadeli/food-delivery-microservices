using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;

public class RestockSubscriptionCustomNotFoundException : CustomNotFoundException
{
    public RestockSubscriptionCustomNotFoundException(long id) : base("RestockSubscription with id: " + id + " not found")
    {
    }
}
