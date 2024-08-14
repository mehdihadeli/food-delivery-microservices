using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class IdentityException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    System.Exception? innerException = null,
    params string[] errors
) : CustomException(message, statusCode, innerException, errors);
