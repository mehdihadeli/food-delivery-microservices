using BuildingBlocks.Core.Web.HeaderPropagation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Web.Middlewares.HeaderPropagation;

public class HeaderPropagationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomHeaderPropagationOptions _options;
    private readonly HeaderPropagationStore _headerPropagationStore;

    public HeaderPropagationMiddleware(
        RequestDelegate next,
        IOptions<CustomHeaderPropagationOptions> options,
        HeaderPropagationStore headerPropagationStore
    )
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(headerPropagationStore);

        _next = next;
        _options = options.Value;
        _headerPropagationStore = headerPropagationStore;
    }

    public Task Invoke(HttpContext context)
    {
        foreach (var headerName in _options.HeaderNames)
        {
            if (!_headerPropagationStore.Headers.ContainsKey(headerName))
            {
                context.Request.Headers.TryGetValue(headerName, out var value);
                if (!StringValues.IsNullOrEmpty(value))
                {
                    _headerPropagationStore.Headers.Add(headerName, value);
                }
            }
        }

        return _next.Invoke(context);
    }
}
