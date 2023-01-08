using BuildingBlocks.Core.Exception.Types;

namespace BuildingBlocks.Core.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Throws an exception if the <see cref="P:System.Net.Http.HttpResponseMessage.IsSuccessStatusCode" /> property for the HTTP response is <see langword="false"/> and read exception detail from response content - default EnsureSuccessStatusCode returns HttpRequestException with no response detail exception
    /// Ref: https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
    /// </summary>
    /// <param name="response">HttpResponseMessage.</param>
    public static async Task EnsureSuccessStatusCodeWithDetailAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();

        throw new HttpResponseException(content, response.StatusCode);
    }
}
