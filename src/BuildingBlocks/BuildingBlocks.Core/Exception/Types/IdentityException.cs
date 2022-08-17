using System.Net;

namespace BuildingBlocks.Core.Exception.Types;

public class IdentityException : CustomException
{
    public IdentityException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        params string[] errors)
        : base(message, statusCode, errors)
    {
    }
}
