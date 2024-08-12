using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.V1;

internal class GetRestockSubscriptionsEndpoint
    : IQueryMinimalEndpoint<
        GetRestockSubscriptionsRequestParameters,
        Ok<GetRestockSubscriptionsResponse>,
        ValidationProblem,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        // return app.MapQueryEndpoint<GetCustomersRequestParameters, GetCustomersResponse, GetCustomers,
        //         GetProductsResult>("/")
        return builder
            .MapGet("/", HandleAsync)
            .RequireAuthorization()
            .WithName(nameof(GetRestockSubscriptions))
            .WithSummaryAndDescription(
                nameof(GetRestockSubscriptions).Humanize(),
                nameof(GetRestockSubscriptions).Humanize()
            )
            .WithDisplayName(nameof(GetRestockSubscriptions).Humanize());

        // .Produces<GetCustomersResponse>("Customers fetched successfully.", StatusCodes.Status200OK)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem(StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<Ok<GetRestockSubscriptionsResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>
    > HandleAsync([AsParameters] GetRestockSubscriptionsRequestParameters requestParameters)
    {
        var (
            pageNumber,
            pageSize,
            filters,
            sortOrder,
            emails,
            from,
            to,
            context,
            queryProcessor,
            mapper,
            cancellationToken
        ) = requestParameters;

        var result = await queryProcessor.SendAsync(
            GetRestockSubscriptions.Of(
                new PageRequest
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Filters = filters,
                    SortOrder = sortOrder
                },
                emails,
                from,
                to
            ),
            cancellationToken
        );

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetRestockSubscriptionsResponse(result.RestockSubscriptions));
    }
}

public record GetRestockSubscriptionsResponse(IPageList<RestockSubscriptionDto> RestockSubscriptions);

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
public record GetRestockSubscriptionsRequestParameters(
    int PageNumber,
    int PageSize,
    string? Filters,
    string? SortOrder,
    [FromBody] IList<string> Emails,
    DateTime? From,
    DateTime? To,
    HttpContext HttpContext,
    IQueryBus QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery, IPageRequest;
