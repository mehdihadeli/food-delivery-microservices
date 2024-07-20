using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class IdentityException : CustomException
{
    public IdentityException(
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        System.Exception? innerException = null,
        params string[] errors
    )
        : base(message, statusCode, innerException, errors) { }
}
