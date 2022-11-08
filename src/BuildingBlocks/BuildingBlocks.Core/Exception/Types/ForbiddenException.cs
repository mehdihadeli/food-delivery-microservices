using System.Net;

namespace BuildingBlocks.Core.Exception.Types;

public class ForbiddenException : IdentityException
{
    public ForbiddenException(string message) : base(message, statusCode: HttpStatusCode.Forbidden)
    {
    }
}
