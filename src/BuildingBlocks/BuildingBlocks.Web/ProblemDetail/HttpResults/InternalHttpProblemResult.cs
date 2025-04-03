using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.ProblemDetail.HttpResults;

public class InternalHttpProblemResult
    : IResult,
        IStatusCodeHttpResult,
        IContentTypeHttpResult,
        IValueHttpResult,
        IEndpointMetadataProvider
{
    private readonly ProblemHttpResult _internalResult;

    internal InternalHttpProblemResult(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);
        if (problemDetails is { Status: not null and not StatusCodes.Status500InternalServerError })
        {
            throw new ArgumentException(
                $"{nameof(InternalHttpProblemResult)} only supports a 500 Internal Server Error response status code.",
                nameof(problemDetails)
            );
        }

        problemDetails.Status ??= StatusCodes.Status500InternalServerError;

        _internalResult = TypedResults.Problem(problemDetails);
    }

    public ProblemDetails ProblemDetails => _internalResult.ProblemDetails;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _internalResult.ExecuteAsync(httpContext);
    }

    public int? StatusCode => _internalResult.StatusCode;
    public string? ContentType => _internalResult.ContentType;
    object? IValueHttpResult.Value => _internalResult.ProblemDetails;

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);
        builder.Metadata.Add(
            new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)
        );
    }
}
