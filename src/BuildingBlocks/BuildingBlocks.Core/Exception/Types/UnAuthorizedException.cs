using System.Net;

namespace BuildingBlocks.Core.Exception.Types;

public class UnAuthorizedException : IdentityException
{
    public UnAuthorizedException(string message) : base(message, HttpStatusCode.Unauthorized)
    {
    }
}
