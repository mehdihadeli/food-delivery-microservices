using System.Diagnostics;
using BuildingBlocks.Abstractions.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.ProblemDetail;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public class ProblemDetailsService(
    IEnumerable<IProblemDetailsWriter> writers,
    IWebHostEnvironment webHostEnvironment,
    ILogger<ProblemDetailsService> logger,
    IEnumerable<IProblemDetailMapper> problemDetailMappers
) : IProblemDetailsService
{
    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ProblemDetails);
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        // Skip if response already started or status code < 400
        if (context.HttpContext.Response.HasStarted || context.HttpContext.Response.StatusCode < 400)
        {
            return ValueTask.CompletedTask;
        }

        // with the help of `capture exception middleware` for capturing actual thrown exception, in .net 8 preview 5 it will create automatically
        IExceptionHandlerFeature? exceptionFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();

        Exception? exception = exceptionFeature?.Error ?? context.Exception;

        // if we throw an exception, we should create appropriate ProblemDetail based on the exception, else we just return default ProblemDetail with status 500 or a custom ProblemDetail which is returned from the endpoint
        CreateProblemDetailFromException(context, exception);

        // Write using the best-matched writer
        foreach (var writer in writers)
        {
            if (writer.CanWrite(context))
            {
                return writer.WriteAsync(context);
            }
        }

        logger.LogWarning("No suitable IProblemDetailsWriter found for the current context.");

        return ValueTask.CompletedTask;
    }

    private void CreateProblemDetailFromException(ProblemDetailsContext context, Exception? exception)
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        if (exception is { })
        {
            logger.LogError(
                exception,
                "Could not process a request on machine {MachineName}. TraceId: {TraceId}",
                Environment.MachineName,
                traceId
            );
        }

        int statusCode = !problemDetailMappers.Any()
            ? new DefaultProblemDetailMapper().GetMappedStatusCodes(exception)
            : problemDetailMappers.Select(m => m.GetMappedStatusCodes(exception)).FirstOrDefault();

        context.HttpContext.Response.StatusCode = statusCode;

        context.ProblemDetails = PopulateNewProblemDetail(statusCode, context.HttpContext, exception, traceId);
    }

    private ProblemDetails PopulateNewProblemDetail(
        int code,
        HttpContext httpContext,
        Exception? exception,
        string traceId
    )
    {
        var extensions = new Dictionary<string, object?> { { "traceId", traceId } };

        // Add stackTrace in development mode for debugging purposes
        if (webHostEnvironment.IsDevelopment() && exception is { })
        {
            extensions["stackTrace"] = exception.StackTrace;
        }

        // type will fill automatically by .net core
        var problem = TypedResults
            .Problem(
                statusCode: code,
                detail: exception?.Message,
                title: exception?.GetType().Name,
                instance: $"{httpContext.Request.Method} {httpContext.Request.Path}",
                extensions: extensions
            )
            .ProblemDetails;

        return problem;
    }
}
