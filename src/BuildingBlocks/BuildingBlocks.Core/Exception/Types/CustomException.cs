using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class CustomException : System.Exception
{
    protected CustomException(
        string message,
        int statusCode = StatusCodes.Status500InternalServerError,
        System.Exception? innerException = null,
        params string[] errors
    )
        : base(message, innerException)
    {
        ErrorMessages = errors;
        StatusCode = statusCode;
    }

    public IEnumerable<string> ErrorMessages { get; protected set; }

    public int StatusCode { get; protected set; }

    // Will use in the problem detail `title` field.
    public override string ToString()
    {
        return GetType().FullName ?? GetType().Name;
    }
}
