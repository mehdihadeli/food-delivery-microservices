using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomers.v1;

internal class GetCustomersEndpoint
    : IQueryMinimalEndpoint<
        GetCustomersRequestParameters,
        Ok<GetCustomersResponse>,
        ValidationProblem,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        // return app.MapQueryEndpoint<GetCustomersRequestParameters, GetCustomersResponse, GetCustomers,
        //         GetProductsResult>("/")
        return builder
            .MapGet("/", HandleAsync)
            .RequireAuthorization()
            .WithName(nameof(GetCustomers))
            .WithSummaryAndDescription(nameof(GetCustomers).Humanize(), nameof(GetCustomers).Humanize())
            .WithDisplayName(nameof(GetCustomers).Humanize());

        // .Produces<GetCustomersResponse>("Customers fetched successfully.", StatusCodes.Status200OK)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem(StatusCodes.Status401Unauthorized)
    }

    public async Task<Results<Ok<GetCustomersResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> HandleAsync(
        [AsParameters] GetCustomersRequestParameters requestParameters
    )
    {
        var (context, queryProcessor, cancellationToken, pageSize, pageNumber, filters, sortOrder) = requestParameters;

        var query = GetCustomers.Of(
            new PageRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder
            }
        );

        var result = await queryProcessor.SendAsync(query, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetCustomersResponse(result.Customers));
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetCustomersRequestParameters(
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken,
    int PageSize = 10,
    int PageNumber = 1,
    string? Filters = null,
    string? SortOrder = null
) : IHttpQuery, IPageRequest;

internal record GetCustomersResponse(IPageList<CustomerReadDto> Customers);
