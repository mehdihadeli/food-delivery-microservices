using BuildingBlocks.Abstractions.Web.Problem;
using Humanizer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.ProblemDetail;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public class ProblemDetailsService(
    IEnumerable<IProblemDetailsWriter> writers,
    IWebHostEnvironment webHostEnvironment,
    IEnumerable<IProblemDetailMapper>? problemDetailMappers = null
) : IProblemDetailsService
{
    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ProblemDetails);
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        // with help of `capture exception middleware` for capturing actual thrown exception, in .net 8 preview 5 it will create automatically
        IExceptionHandlerFeature? exceptionFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();

        // if we throw an exception, we should create appropriate ProblemDetail based on the exception, else we just return default ProblemDetail with status 500 or a custom ProblemDetail which is returned from the endpoint
        if (exceptionFeature is not null)
        {
            CreateProblemDetailFromException(context, exceptionFeature);
        }

        if (context.HttpContext.Response.HasStarted || context.HttpContext.Response.StatusCode < 400 || !writers.Any())
            return ValueTask.CompletedTask;

        IProblemDetailsWriter problemDetailsWriter = null!;
        if (writers.Count() == 1)
        {
            IProblemDetailsWriter writer = writers.First();
            return !writer.CanWrite(context) ? ValueTask.CompletedTask : writer.WriteAsync(context);
        }

        foreach (var writer in writers)
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
        if (problemDetailMappers is not null && problemDetailMappers.Any())
        {
            foreach (var problemDetailMapper in problemDetailMappers)
            {
                MapProblemDetail(context, exceptionFeature, problemDetailMapper);
            }
        }
        else
        {
            var defaultMapper = new DefaultProblemDetailMapper();
            MapProblemDetail(context, exceptionFeature, defaultMapper);
        }
    }

    private void MapProblemDetail(
        ProblemDetailsContext context,
        IExceptionHandlerFeature exceptionFeature,
        IProblemDetailMapper problemDetailMapper
    )
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

    private void PopulateNewProblemDetail(
        ProblemDetails existingProblemDetails,
        HttpContext httpContext,
        int statusCode,
        Exception exception
    )
    {
        // We should override ToString method in the exception for showing correct title.
        existingProblemDetails.Title = exception.GetType().Name.Humanize(LetterCasing.Title);
        existingProblemDetails.Detail = exception.Message;
        existingProblemDetails.Status = statusCode;
        existingProblemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";

        if (webHostEnvironment.IsDevelopment())
        {
            existingProblemDetails.Extensions = new Dictionary<string, object?>
            {
                { "stackTrace", exception.StackTrace },
            };
        }
    }
}
