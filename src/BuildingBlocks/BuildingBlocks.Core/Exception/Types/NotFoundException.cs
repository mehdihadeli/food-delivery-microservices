using BuildingBlocks.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class NotFoundException : CustomException
{
    public NotFoundException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status404NotFound, innerException) { }
}

public class NotFoundAppException : AppException
{
    public NotFoundAppException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status404NotFound, innerException) { }
}

public class NotFoundDomainException : DomainException
{
    public NotFoundDomainException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public NotFoundDomainException(Type businessRuleType, string message)
        : base(businessRuleType, message, StatusCodes.Status404NotFound) { }
}
