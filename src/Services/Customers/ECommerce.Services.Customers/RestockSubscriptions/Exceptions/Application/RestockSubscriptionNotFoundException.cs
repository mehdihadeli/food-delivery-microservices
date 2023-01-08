using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;

public class RestockSubscriptionNotFoundException : AppException
{
    public RestockSubscriptionNotFoundException(long id) : base(
        $"RestockSubscription with id: {id}not found",
        HttpStatusCode.NotFound)
    {
    }

    public RestockSubscriptionNotFoundException(Guid id) : base(
        $"RestockSubscription with id: {id}not found",
        HttpStatusCode.NotFound)
    {
    }
}
