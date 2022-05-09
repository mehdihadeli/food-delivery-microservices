using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Exceptions;

public class ProductAlreadySubscribedException : AppException
{
    public ProductAlreadySubscribedException(long productId, string productName)
        : base($"Product with Id '{productId}' and Name '{productName}' is already subscribed", HttpStatusCode.Conflict)
    {
    }
}
