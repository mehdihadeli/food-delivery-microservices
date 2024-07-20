using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.GetRestockSubscriptionById.v1;

internal class GetRestockSubscriptionByIdEndpoint
    : IQueryMinimalEndpoint<
        GetRestockSubscriptionByIdRequestParameters,
        Ok<GetRestockSubscriptionByIdResponse>,
        ValidationProblem,
        NotFoundHttpProblemResult,
        UnAuthorizedHttpProblemResult
    >
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapGet("/{id:guid}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            // .Produces<GetRestockSubscriptionByIdResponse>(StatusCodes.Status200OK)
            // .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            // .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            // .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithName(nameof(GetRestockSubscriptionById))
            .WithDisplayName(nameof(GetRestockSubscriptionById).Humanize())
            .WithSummaryAndDescription(
                nameof(GetRestockSubscriptionById).Humanize(),
                nameof(GetRestockSubscriptionById).Humanize()
            );
    }

    public async Task<
        Results<
            Ok<GetRestockSubscriptionByIdResponse>,
            ValidationProblem,
            NotFoundHttpProblemResult,
            UnAuthorizedHttpProblemResult
        >
    > HandleAsync([AsParameters] GetRestockSubscriptionByIdRequestParameters requestParameters)
    {
        var (id, _, queryProcessor, mapper, cancellationToken) = requestParameters;
        var result = await queryProcessor.SendAsync(GetRestockSubscriptionById.Of(id), cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.Ok(new GetRestockSubscriptionByIdResponse(result.RestockSubscription));
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetRestockSubscriptionByIdRequestParameters(
    [FromRoute] Guid Id,
    HttpContext HttpContext,
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpQuery;

public record GetRestockSubscriptionByIdResponse(RestockSubscriptionDto RestockSubscription);
