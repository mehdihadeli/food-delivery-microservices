using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

// https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
public class HttpResponseException : CustomException
{
    public string? ResponseContent { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>>? Headers { get; }

    public HttpResponseException(
        string responseContent,
        int statusCode = StatusCodes.Status500InternalServerError,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
        System.Exception? inner = null
    )
        : base(responseContent, statusCode, inner)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        Headers = headers;
    }

    public override string ToString()
    {
        return $"HTTP Response: \n\n{ResponseContent}\n\n{base.ToString()}";
    }
}
