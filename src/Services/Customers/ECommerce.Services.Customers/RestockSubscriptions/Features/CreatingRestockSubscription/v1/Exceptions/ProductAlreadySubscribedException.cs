using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;

public class ProductAlreadySubscribedException : AppException
{
    public ProductAlreadySubscribedException(long productId, string productName)
        : base($"Product with InternalCommandId '{productId}' and Name '{productName}' is already subscribed", HttpStatusCode.Conflict)
    {
    }
}
