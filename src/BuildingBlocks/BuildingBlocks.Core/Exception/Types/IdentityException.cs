using System.Net;

namespace BuildingBlocks.Core.Exception.Types;

public class IdentityException : CustomException
{
    public IdentityException(string message, List<string> errors = default, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, statusCode, errors)
    {
    }
}
