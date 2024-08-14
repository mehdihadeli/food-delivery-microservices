using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class ApiException : CustomException
{
    public ApiException(string message, int statusCode = StatusCodes.Status500InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
