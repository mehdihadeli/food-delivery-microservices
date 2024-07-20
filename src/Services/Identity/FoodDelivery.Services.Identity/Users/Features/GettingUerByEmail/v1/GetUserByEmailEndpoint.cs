using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUerByEmail.v1;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
public static class GetUserByEmailEndpoint
{
    internal static RouteHandlerBuilder MapGetUserByEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/by-email/{email}", Handle)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .WithName(nameof(GetUserByEmail))
            .WithDisplayName(nameof(GetUserByEmail).Humanize())
            .WithSummaryAndDescription(nameof(GetUserByEmail).Humanize(), nameof(GetUserByEmail).Humanize())
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<RegisterUserResponse>(StatusCodes.Status200OK)
            // .ProducesProblem(StatusCodes.Status404NotFound)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetUserByEmailResponse>, ValidationProblem, NotFoundHttpProblemResult>> Handle(
            [AsParameters] GetUserByEmailRequestParameters requestParameters
        )
        {
            var (email, _, queryProcessor, mapper, cancellationToken) = requestParameters;
            var result = await queryProcessor.SendAsync(GetUserByEmail.Of(email), cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetUserByEmailResponse(result.UserIdentity));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetUserByEmailRequestParameters(
    [FromRoute] string Email,
    HttpContext HttpContext,
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery;

internal record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
