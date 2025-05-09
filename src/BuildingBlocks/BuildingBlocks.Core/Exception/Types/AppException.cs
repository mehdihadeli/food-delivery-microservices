using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class AppException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    System.Exception? innerException = null
) : CustomException(message, statusCode, innerException){
    
    // Helper for 400 Bad Request
    public static AppException BadRequest(string message, System.Exception? innerException = null) 
        => new(message,StatusCodes.Status400BadRequest, innerException);

    // Helper for 404 Not Found
    public static AppException NotFound(string message, System.Exception? innerException = null) 
        => new(message, StatusCodes.Status404NotFound, innerException);
}

