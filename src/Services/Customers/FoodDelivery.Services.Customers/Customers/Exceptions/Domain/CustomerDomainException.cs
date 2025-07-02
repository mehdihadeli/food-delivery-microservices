using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

public class CustomerDomainException(string message, int statusCode = StatusCodes.Status400BadRequest)
    : DomainException(message, statusCode);
