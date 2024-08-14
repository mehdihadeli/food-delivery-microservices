using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class AppException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    System.Exception? innerException = null
) : CustomException(message, statusCode, innerException);
