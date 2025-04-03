using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Identity.Features.GettingClaims.v1;

internal static class GetClaimsEndpoint
{
    internal static RouteHandlerBuilder MapGetClaimsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/claims", Handle)
            .RequireAuthorization()
            .WithTags(IdentityConfigurations.Tag)
            .WithName(nameof(GetClaims))
            .WithSummary(nameof(GetClaims).Humanize())
            .WithDescription(nameof(GetClaims).Humanize())
            .WithDisplayName(nameof(GetClaims).Humanize())
            .MapToApiVersion(1.0);

        // // Api Documentations will produce automatically by typed result in minimal apis.
        // // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
        // .Produces<GetClaimsResponse>(statusCode: StatusCodes.Status200OK)
        // .ProducesProblem(StatusCodes.Status401Unauthorized);
        async Task<Results<Ok<GetClaimsResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> Handle(
            [AsParameters] GetClaimsRequestParameters requestParameters
        )
        {
            var (context, queryProcessor, cancellationToken) = requestParameters;
            var result = await queryProcessor.SendAsync(GetClaims.Of(), cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetClaimsResponse(result.Claims));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetClaimsRequestParameters(
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken
) : IHttpQuery;

internal record GetClaimsResponse(IEnumerable<ClaimDto>? Claims);
