using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception;

public class UnAuthorizedException(string message, System.Exception? innerException = null)
    : IdentityException(message, StatusCodes.Status401Unauthorized, innerException);
