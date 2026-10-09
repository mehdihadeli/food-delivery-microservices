using Squirrel.Core.Domain.Exceptions;
using Squirrel.Core.Exception;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message)
        : base(message) { }
}
