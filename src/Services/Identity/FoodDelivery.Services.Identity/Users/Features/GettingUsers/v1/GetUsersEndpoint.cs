using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Shared;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUsers.v1;

internal static class GetUsersEndpoint
{
    internal static RouteHandlerBuilder MapGetUsersByPageEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/", Handle)
            .RequireAuthorization(policyNames: [Authorization.ClientPermissions.UserRead])
            .WithTags(UsersConfigurations.Tag)
            .WithName(nameof(GetUsers))
            .WithDescription(nameof(GetUsers).Humanize())
            .WithSummary(nameof(GetUsers).Humanize())
            .WithDisplayName(nameof(GetUsers).Humanize())
            // Api Documentations will produce automatically by typed result in minimal apis.
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<GetProductsResponse>("Products fetched successfully.", StatusCodes.Status200OK)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetUsersResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> Handle(
            [AsParameters] GetUsersRequestParameters requestParameters
        )
        {
            var (context, queryProcessor, cancellationToken) = requestParameters;

            var query = GetUsers.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    SortOrder = requestParameters.SortOrder,
                    Filters = requestParameters.SortOrder,
                }
            );

            var result = await queryProcessor.SendAsync(query, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetUsersResponse(result.IdentityUsers));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetUsersRequestParameters(
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken
) : PageRequest, IHttpQuery;

internal record GetUsersResponse(IPageList<IdentityUserDto> IdentityUsers);
