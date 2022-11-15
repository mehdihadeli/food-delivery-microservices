using System.Net;

namespace BuildingBlocks.Core.Exception.Types;

public class CustomNotFoundException : CustomException
{
    public CustomNotFoundException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.NotFound;
    }
}
