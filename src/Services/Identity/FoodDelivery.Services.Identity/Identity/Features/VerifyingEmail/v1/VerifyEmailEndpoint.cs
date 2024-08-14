using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1;

public static class VerifyEmailEndpoint
{
    internal static RouteHandlerBuilder MapSendVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/verify-email", Handle)
            .AllowAnonymous()
            .MapToApiVersion(1.0)
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesProblem(StatusCodes.Status409Conflict)
            // .ProducesProblem(StatusCodes.Status500InternalServerError)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(VerifyEmail))
            .WithDisplayName(nameof(VerifyEmail).Humanize())
            .WithSummaryAndDescription(nameof(VerifyEmail).Humanize(), nameof(VerifyEmail).Humanize());

        async Task<Results<NoContent, ConflictHttpProblemResult, InternalHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] VerifyEmailRequestParameters requestParameters
        )
        {
            var (request, context, commandBus, mapper, cancellationToken) = requestParameters;

            var command = VerifyEmail.Of(request.Email, request.Code);

            await commandBus.SendAsync(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

internal record VerifyEmailRequest(string? Email, string? Code);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record VerifyEmailRequestParameters(
    [FromBody] VerifyEmailRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<VerifyEmailRequest>;
