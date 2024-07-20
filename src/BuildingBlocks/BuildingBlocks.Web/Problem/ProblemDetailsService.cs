using BuildingBlocks.Abstractions.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.Problem;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public class ProblemDetailsService : IProblemDetailsService
{
    private readonly IEnumerable<IProblemDetailMapper>? _problemDetailMappers;
    private readonly IProblemDetailsWriter[] _writers;

    public ProblemDetailsService(
        IEnumerable<IProblemDetailsWriter> writers,
        IEnumerable<IProblemDetailMapper>? problemDetailMappers = null
    )
    {
        _writers = writers.ToArray();
        _problemDetailMappers = problemDetailMappers;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull((object)context, nameof(context));
        ArgumentNullException.ThrowIfNull((object)context.ProblemDetails, "context.ProblemDetails");
        ArgumentNullException.ThrowIfNull((object)context.HttpContext, "context.HttpContext");

        // with help of `capture exception middleware` for capturing actual thrown exception, in .net 8 preview 5 it will create automatically
        IExceptionHandlerFeature? exceptionFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();

        // if we throw an exception, we should create appropriate ProblemDetail based on the exception, else we just return default ProblemDetail with status 500 or a custom ProblemDetail which is returned from the endpoint
        if (exceptionFeature is not null)
        {
            CreateProblemDetailFromException(context, exceptionFeature);
        }

        if (
            context.HttpContext.Response.HasStarted
            || context.HttpContext.Response.StatusCode < 400
            || _writers.Length == 0
        )
            return ValueTask.CompletedTask;

        IProblemDetailsWriter problemDetailsWriter = null!;
        if (_writers.Length == 1)
        {
            IProblemDetailsWriter writer = _writers[0];
            return !writer.CanWrite(context) ? ValueTask.CompletedTask : writer.WriteAsync(context);
        }

        foreach (var writer in _writers)
        {
            if (writer.CanWrite(context))
            {
                problemDetailsWriter = writer;
                break;
            }
        }

        return problemDetailsWriter?.WriteAsync(context) ?? ValueTask.CompletedTask;
    }

    private void CreateProblemDetailFromException(
        ProblemDetailsContext context,
        IExceptionHandlerFeature exceptionFeature
    )
    {
        if (_problemDetailMappers is { })
        {
            foreach (var problemDetailMapper in _problemDetailMappers)
            {
                var mappedStatusCode = problemDetailMapper.GetMappedStatusCodes(exceptionFeature.Error);
                if (mappedStatusCode > 0)
                {
                    PopulateNewProblemDetail(
                        context.ProblemDetails,
                        context.HttpContext,
                        mappedStatusCode,
                        exceptionFeature.Error
                    );
                    context.HttpContext.Response.StatusCode = mappedStatusCode;
                }
            }
        }
    }

    private static void PopulateNewProblemDetail(
        ProblemDetails existingProblemDetails,
        HttpContext httpContext,
        int statusCode,
        Exception exception
    )
    {
        // We should override ToString method in the exception for showing correct title.
        existingProblemDetails.Title = exception.ToString();
        existingProblemDetails.Detail = exception.Message;
        existingProblemDetails.Status = statusCode;
        existingProblemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
    }
}
