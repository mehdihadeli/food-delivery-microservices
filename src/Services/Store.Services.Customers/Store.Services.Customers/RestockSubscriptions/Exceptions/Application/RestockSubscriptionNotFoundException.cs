using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.RestockSubscriptions.Exceptions.Application;

public class RestockSubscriptionNotFoundException : NotFoundException
{
    public RestockSubscriptionNotFoundException(long id) : base("RestockSubscription with id: " + id + " not found")
    {
    }
}
