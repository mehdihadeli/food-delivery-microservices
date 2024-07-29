using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Core.Web.HeaderPropagation;

public class HeaderPropagationMessageHandler : DelegatingHandler
{
    private readonly CustomHeaderPropagationOptions _options;
    private readonly CustomHeaderPropagationStore _customHeaderPropagationStore;

    public HeaderPropagationMessageHandler(CustomHeaderPropagationOptions options, CustomHeaderPropagationStore headers)
    {
        _options = options;
        _customHeaderPropagationStore = headers;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        System.Threading.CancellationToken cancellationToken
    )
    {
        foreach (var headerName in _options.HeaderNames)
        {
            // Get the incoming header value
            _customHeaderPropagationStore.Headers.TryGetValue(headerName, out var headerValue);
            if (StringValues.IsNullOrEmpty(headerValue))
            {
                continue;
            }

            request.Headers.TryAddWithoutValidation(headerName, headerValue.ToArray());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
