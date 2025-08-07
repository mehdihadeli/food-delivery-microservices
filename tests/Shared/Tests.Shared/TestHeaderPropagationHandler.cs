using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Primitives;

namespace Tests.Shared;

public class TestHeaderPropagationHandler(HeaderPropagationValues values) : DelegatingHandler
{
    private readonly HeaderPropagationValues _values = values ?? throw new ArgumentNullException(nameof(values));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // We need to initialize the headers if not done by middleware (which doesn't run in integration tests)
        _values.Headers ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

        // Optionally inject test headers if needed
        if (!_values.Headers.ContainsKey("X-Correlation-ID"))
        {
            _values.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();
        }

        return base.SendAsync(request, cancellationToken);
    }
}
