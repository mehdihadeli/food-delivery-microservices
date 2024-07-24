using BuildingBlocks.Web.HeaderPropagation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Web.Middlewares.HeaderPropagation;

public class HeaderPropagationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomHeaderPropagationOptions _options;
    private readonly CustomHeaderPropagationStore _customHeaderPropagationStore;

    public HeaderPropagationMiddleware(
        RequestDelegate next,
        IOptions<CustomHeaderPropagationOptions> options,
        CustomHeaderPropagationStore customHeaderPropagationStore
    )
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(customHeaderPropagationStore);

        _next = next;
        _options = options.Value;
        _customHeaderPropagationStore = customHeaderPropagationStore;
    }

    public Task Invoke(HttpContext context)
    {
        foreach (var headerName in _options.HeaderNames)
        {
            if (!_customHeaderPropagationStore.Headers.ContainsKey(headerName))
            {
                context.Request.Headers.TryGetValue(headerName, out var value);
                if (!StringValues.IsNullOrEmpty(value))
                {
                    _customHeaderPropagationStore.Headers.Add(headerName, value);
                }
            }
        }

        return _next.Invoke(context);
    }
}
