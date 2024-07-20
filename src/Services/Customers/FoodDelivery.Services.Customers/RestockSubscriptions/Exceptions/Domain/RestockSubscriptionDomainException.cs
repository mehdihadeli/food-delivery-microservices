using BuildingBlocks.Core.Domain.Exceptions;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message)
        : base(message) { }
}
