using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUsers.v1;

internal static class GetUsersEndpoint
{
    internal static RouteHandlerBuilder MapGetUsersByPageEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/", Handle)
            .RequireAuthorization()
            .WithTags(UsersConfigs.Tag)
            .WithName(nameof(GetUsers))
            .WithSummaryAndDescription(nameof(GetUsers).Humanize(), nameof(GetUsers).Humanize())
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
            var (context, queryProcessor, mapper, cancellationToken) = requestParameters;

            var query = GetUsers.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    SortOrder = requestParameters.SortOrder,
                    Filters = requestParameters.SortOrder
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
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : PageRequest, IHttpQuery;

internal record GetUsersResponse(IPageList<IdentityUserDto> IdentityUsers);
