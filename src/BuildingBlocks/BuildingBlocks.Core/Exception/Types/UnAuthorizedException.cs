using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class UnAuthorizedException : IdentityException
{
    public UnAuthorizedException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status401Unauthorized, innerException) { }
}
