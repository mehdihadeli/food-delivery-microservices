using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Identity.Shared.Models;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1;

internal static class UpdateUserStateEndpoint
{
    internal static RouteHandlerBuilder MapUpdateUserStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPut("/{userId:guid}/state", Handle)
            .AllowAnonymous()
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RegisterUserResponse>(StatusCodes.Status204NoContent)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(UpdateUserState))
            .WithSummaryAndDescription(nameof(UpdateUserState).Humanize(), nameof(UpdateUserState).Humanize())
            .WithDisplayName(nameof(UpdateUserState).Humanize())
            .MapToApiVersion(1.0);

        async Task<Results<NoContent, NotFoundHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] UpdateUserStateRequestParameters requestParameters
        )
        {
            var (request, userId, context, commandProcessor, mapper, cancellationToken) = requestParameters;
            var command = UpdateUserState.Of(userId, request.UserState);

            await commandProcessor.SendAsync(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record UpdateUserStateRequestParameters(
    [FromBody] UpdateUserStateRequest Request,
    [FromRoute] Guid UserId,
    HttpContext HttpContext,
    ICommandProcessor CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateUserStateRequest>;

internal record UpdateUserStateRequest(UserState UserState);
