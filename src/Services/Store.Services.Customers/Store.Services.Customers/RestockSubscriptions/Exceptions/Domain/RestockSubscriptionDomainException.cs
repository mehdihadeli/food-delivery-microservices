using BuildingBlocks.Core.Domain.Exceptions;

namespace Store.Services.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message) : base(message)
    {
    }
}
