using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class AppException : CustomException
{
    public AppException(
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        System.Exception? innerException = null
    )
        : base(message, statusCode, innerException) { }
}
