using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.ProblemDetail.HttpResults;

public class ConflictHttpProblemResult
    : IResult,
        IStatusCodeHttpResult,
        IContentTypeHttpResult,
        IValueHttpResult,
        IEndpointMetadataProvider
{
    private readonly ProblemHttpResult _internalResult;

    internal ConflictHttpProblemResult(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);
        if (problemDetails is { Status: not null and not StatusCodes.Status409Conflict })
        {
            throw new ArgumentException(
                $"{nameof(ConflictHttpProblemResult)} only supports a 409 Conflict response status code.",
                nameof(problemDetails)
            );
        }

        problemDetails.Status ??= StatusCodes.Status409Conflict;

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
        builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status409Conflict));
    }
}
