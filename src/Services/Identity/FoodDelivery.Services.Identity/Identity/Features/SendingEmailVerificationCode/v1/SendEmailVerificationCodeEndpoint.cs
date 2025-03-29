using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;

public static class SendEmailVerificationCodeEndpoint
{
    internal static RouteHandlerBuilder MapSendEmailVerificationCodeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/send-email-verification-code", Handle)
            .AllowAnonymous()
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesProblem(StatusCodes.Status409Conflict)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(SendEmailVerificationCode))
            .WithDisplayName(nameof(SendEmailVerificationCode).Humanize())
            .WithSummary(nameof(SendEmailVerificationCode).Humanize())
            .WithDescription(nameof(SendEmailVerificationCode).Humanize());

        async Task<Results<NoContent, ConflictHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] SendEmailVerificationCodeRequestParameters requestParameters
        )
        {
            var (request, context, commandBus, cancellationToken) = requestParameters;
            var command = SendEmailVerificationCode.Of(request.Email);

            await commandBus.SendAsync(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

public record SendEmailVerificationCodeRequest(string? Email);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record SendEmailVerificationCodeRequestParameters(
    [FromBody] SendEmailVerificationCodeRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<SendEmailVerificationCodeRequest>;
