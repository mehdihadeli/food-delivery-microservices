using CloudNativeKit.Core.Domain.Exceptions;
using CloudNativeKit.Core.Exception;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message)
        : base(message) { }
}
