using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

internal class GetRestockSubscriptionsEndpoint
    : IQueryMinimalEndpoint<
        GetRestockSubscriptionsRequestParameters,
        Ok<GetRestockSubscriptionsResponse>,
        ValidationProblem,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => RestockSubscriptionsConfigurations.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigurations.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        // return app.MapQueryEndpoint<GetCustomersRequestParameters, GetCustomersResponse, GetCustomers,
        //         GetProductsResult>("/")
        return builder
            .MapGet("/", HandleAsync)
            .RequireAuthorization()
            .WithName(nameof(GetRestockSubscriptions))
            .WithDescription(nameof(GetRestockSubscriptions).Humanize())
            .WithSummary(nameof(GetRestockSubscriptions).Humanize())
            .WithDisplayName(nameof(GetRestockSubscriptions).Humanize());

        // .Produces<GetCustomersResponse>("Customers fetched successfully.", StatusCodes.Status200OK)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem(StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<Ok<GetRestockSubscriptionsResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>
    > HandleAsync([AsParameters] GetRestockSubscriptionsRequestParameters requestParameters)
    {
        var result = await requestParameters.QueryBus.SendAsync(
            GetRestockSubscriptions.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    Filters = requestParameters.Filters,
                    SortOrder = requestParameters.SortOrder,
                },
                [requestParameters.Email],
                requestParameters.From,
                requestParameters.To
            ),
            requestParameters.CancellationToken
        );

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetRestockSubscriptionsResponse(result.RestockSubscriptions));
    }
}

public record GetRestockSubscriptionsResponse(IPageList<RestockSubscriptionDto> RestockSubscriptions);

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
internal record GetRestockSubscriptionsRequestParameters(
    string Email,
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken,
    int PageSize = 10,
    int PageNumber = 1,
    string? Filters = null,
    string? SortOrder = null,
    DateTime? From = null,
    DateTime? To = null
) : IHttpQuery, IPageRequest;
