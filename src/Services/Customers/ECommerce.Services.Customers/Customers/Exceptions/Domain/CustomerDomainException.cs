using System.Net;
using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Services.Customers.Customers.Exceptions.Domain;

public class CustomerDomainException : DomainException
{
    public CustomerDomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) :
        base(message, statusCode)
    {
    }
}
