using AutoMapper;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;

internal class GetCustomerByIdEndpointEndpoint
    : IQueryMinimalEndpoint<
        GetCustomerByIdRequestParameters,
        Ok<GetCustomerByIdResponse>,
        ValidationProblem,
        NotFoundHttpProblemResult,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapGet("/{id:guid}", HandleAsync)
            .RequireAuthorization()
            .RequireAuthorization()
            .WithName(nameof(GetCustomerById))
            .WithDisplayName(nameof(GetCustomerById).Humanize())
            .WithSummaryAndDescription(nameof(GetCustomerById).Humanize(), nameof(GetCustomerById).Humanize());

        // .Produces<GetCustomerByIdResponse>("Customer fetched successfully.", StatusCodes.Status200OK)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem(StatusCodes.Status404NotFound)
        // .ProducesProblem(StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<
            Ok<GetCustomerByIdResponse>,
            ValidationProblem,
            NotFoundHttpProblemResult,
            UnAuthorizedHttpProblemResult
        >
    > HandleAsync([AsParameters] GetCustomerByIdRequestParameters requestParameters)
    {
        var (id, _, queryProcessor, mapper, cancellationToken) = requestParameters;
        var result = await queryProcessor.SendAsync(GetCustomerById.Of(id), cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetCustomerByIdResponse(result.Customer));
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetCustomerByIdRequestParameters(
    [FromRoute] Guid Id,
    HttpContext HttpContext,
    IQueryBus QueryBus,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery;

public record GetCustomerByIdResponse(CustomerReadDto Customer);
