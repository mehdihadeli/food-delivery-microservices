using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception;

public class NotFoundException(string message, System.Exception? innerException = null)
    : CustomException(message, StatusCodes.Status404NotFound, innerException);

public class NotFoundAppException(string message, System.Exception? innerException = null)
    : AppException(message, StatusCodes.Status404NotFound, innerException);

public class NotFoundDomainException : DomainException
{
    public NotFoundDomainException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public NotFoundDomainException(Type businessRuleType, string message)
        : base(businessRuleType, message, StatusCodes.Status404NotFound) { }
}
