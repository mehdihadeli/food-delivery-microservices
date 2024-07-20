using BuildingBlocks.Web.Problem.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.Minimal.Extensions;

public static class TypedResultsExtensions
{
    public static InternalHttpProblemResult InternalProblem(
        string? title = null,
        string? detail = null,
        string? instance = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null
    )
    {
        var problemDetails = CreateProblem(title, detail, instance, type, extensions);

        return new(problemDetails);
    }

    public static UnAuthorizedHttpProblemResult UnAuthorizedProblem(
        string? title = null,
        string? detail = null,
        string? instance = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null
    )
    {
        var problemDetails = CreateProblem(title, detail, instance, type, extensions);

        return new(problemDetails);
    }

    private static ProblemDetails CreateProblem(
        string? title,
        string? detail,
        string? instance,
        string? type,
        IDictionary<string, object?>? extensions
    )
    {
        var problemDetails = new ProblemDetails
        {
            Detail = detail,
            Instance = instance,
            Type = type
        };

        problemDetails.Title = title ?? problemDetails.Title;

        if (extensions is not null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions.Add(extension);
            }
        }

        return problemDetails;
    }
}
