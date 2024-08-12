using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Web.HeaderPropagation;

internal class HeaderPropagationMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly CustomHeaderPropagationOptions _options;
    private readonly HeaderPropagationStore _headerPropagationStore;

    public HeaderPropagationMessageHandlerBuilderFilter(
        IOptions<CustomHeaderPropagationOptions> options,
        HeaderPropagationStore header
    )
    {
        _options = options.Value;
        _headerPropagationStore = header;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            builder.AdditionalHandlers.Add(new HeaderPropagationMessageHandler(_options, _headerPropagationStore));
            next(builder);
        };
    }
}
