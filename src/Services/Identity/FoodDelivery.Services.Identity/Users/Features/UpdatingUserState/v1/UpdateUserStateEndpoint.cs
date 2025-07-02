using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Identity.Users;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1;

internal static class UpdateUserStateEndpoint
{
    internal static RouteHandlerBuilder MapUpdateUserStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPut("/{userId:guid}/state", Handle)
            .RequireAuthorization(policyNames: [Authorization.ClientPermissions.UserWrite])
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RegisterUserResponse>(StatusCodes.Status204NoContent)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(UpdateUserState))
            .WithSummary(nameof(UpdateUserState).Humanize())
            .WithDescription(nameof(UpdateUserState).Humanize())
            .WithDisplayName(nameof(UpdateUserState).Humanize())
            .MapToApiVersion(1.0);

        async Task<Results<NoContent, NotFoundHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] UpdateUserStateRequestParameters requestParameters
        )
        {
            var (request, userId, context, commandBus, cancellationToken) = requestParameters;
            var command = UpdateUserState.Of(userId, request.UserState);

            await commandBus.SendAsync(command, cancellationToken);

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
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateUserStateRequest>;

internal record UpdateUserStateRequest(UserState UserState);
