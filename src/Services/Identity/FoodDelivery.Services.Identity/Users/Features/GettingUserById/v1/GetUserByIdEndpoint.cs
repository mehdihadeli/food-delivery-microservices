using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Shared;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUserById.v1;

public static class GetUserByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/{userId:guid}", Handle)
            .RequireAuthorization(policyNames: [Authorization.ClientPermissions.UserRead])
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RegisterUserResponse>(StatusCodes.Status200OK)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithName(nameof(GetUserById))
            .WithDisplayName(nameof(GetUserById).Humanize())
            .WithSummary(nameof(GetUserById).Humanize())
            .WithDescription(nameof(GetUserById).Humanize())
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetUserByIdResponse>, ValidationProblem, NotFoundHttpProblemResult>> Handle(
            [AsParameters] GetUserByIdRequestParameters requestParameters
        )
        {
            var (userId, _, queryProcessor, cancellationToken) = requestParameters;
            var result = await queryProcessor.SendAsync(GetUserById.Of(userId), cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetUserByIdResponse(result.IdentityUser));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetUserByIdRequestParameters(
    [FromRoute] Guid UserId,
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken
) : IHttpQuery;

internal record GetUserByIdResponse(IdentityUserDto? UserIdentity);
