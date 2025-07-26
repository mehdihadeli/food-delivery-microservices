using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Web.ProblemDetail;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/#toc_3
public class ProblemDetailsWriter(IOptions<ProblemDetailsOptions> options) : IProblemDetailsWriter
{
    private readonly ProblemDetailsOptions _options = options.Value;

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;

        // Apply final customization (if any)
        _options.CustomizeProblemDetails?.Invoke(context);

        // Ensure required fields are set
        context.ProblemDetails.Status ??= httpContext.Response.StatusCode;

        // Write JSON response
        return new ValueTask(
            httpContext.Response.WriteAsJsonAsync(
                context.ProblemDetails,
                options: null,
                contentType: "application/problem+json"
            )
        );
    }

    public bool CanWrite(ProblemDetailsContext context)
    {
        return true;
    }
}
