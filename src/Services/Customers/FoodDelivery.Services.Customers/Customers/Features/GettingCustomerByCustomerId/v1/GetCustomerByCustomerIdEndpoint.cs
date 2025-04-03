using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;

internal class GetCustomerByCustomerIdEndpointEndpoint
    : IQueryMinimalEndpoint<
        GetCustomerByCustomerIdRequestParameters,
        Ok<GetCustomerByCustomerIdResponse>,
        ValidationProblem,
        NotFoundHttpProblemResult,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => CustomersConfigurations.Tag;
    public string PrefixRoute => CustomersConfigurations.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapGet("/{customerId}", HandleAsync)
            // .RequireAuthorization()
            .WithName(nameof(GetCustomerByCustomerId))
            .WithDisplayName(nameof(GetCustomerByCustomerId).Humanize())
            .WithSummary(nameof(GetCustomerByCustomerId).Humanize())
            .WithDescription(nameof(GetCustomerByCustomerId).Humanize());

        // .Produces<GetCustomerByCustomerIdResponse>("CustomerReadModel fetched successfully.", StatusCodes.Status200OK)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem(StatusCodes.Status404NotFound)
        // .ProducesProblem(StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<
            Ok<GetCustomerByCustomerIdResponse>,
            ValidationProblem,
            NotFoundHttpProblemResult,
            UnAuthorizedHttpProblemResult
        >
    > HandleAsync([AsParameters] GetCustomerByCustomerIdRequestParameters requestParameters)
    {
        var (id, _, queryProcessor, cancellationToken) = requestParameters;
        var result = await queryProcessor.SendAsync(GetCustomerByCustomerId.Of(id), cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetCustomerByCustomerIdResponse(result.Customer));
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetCustomerByCustomerIdRequestParameters(
    [FromRoute] long CustomerId,
    HttpContext HttpContext,
    IQueryBus QueryBus,
    CancellationToken CancellationToken
) : IHttpQuery;

internal record GetCustomerByCustomerIdResponse(CustomerReadDto Customer);
