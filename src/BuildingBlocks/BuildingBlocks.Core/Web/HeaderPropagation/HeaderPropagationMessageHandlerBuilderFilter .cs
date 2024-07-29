using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Web.HeaderPropagation;

internal class HeaderPropagationMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly CustomHeaderPropagationOptions _options;
    private readonly CustomHeaderPropagationStore _customHeaderPropagationStore;

    public HeaderPropagationMessageHandlerBuilderFilter(
        IOptions<CustomHeaderPropagationOptions> options,
        CustomHeaderPropagationStore header
    )
    {
        _options = options.Value;
        _customHeaderPropagationStore = header;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            builder.AdditionalHandlers.Add(
                new HeaderPropagationMessageHandler(_options, _customHeaderPropagationStore)
            );
            next(builder);
        };
    }
}
