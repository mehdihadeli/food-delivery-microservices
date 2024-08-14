using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomers.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

internal class CreateRestockSubscriptionEndpoint
    : ICommandMinimalEndpoint<
        CreateRestockSubscriptionRequest,
        CreateRestockSubscriptionRequestParameters,
        CreatedAtRoute<CreateRestockSubscriptionResponse>,
        UnAuthorizedHttpProblemResult,
        ValidationProblem
    >
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            .AllowAnonymous()
            // .Produces<CreateRestockSubscriptionResponse>(StatusCodes.Status201Created)
            // .ProducesValidationProblem()
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(CreateRestockSubscription))
            .WithSummaryAndDescription(
                nameof(CreateRestockSubscription).Humanize(),
                nameof(CreateRestockSubscription).Humanize()
            )
            .WithDisplayName(nameof(GetCustomers).Humanize());
    }

    public async Task<
        Results<CreatedAtRoute<CreateRestockSubscriptionResponse>, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync([AsParameters] CreateRestockSubscriptionRequestParameters requestParameters)
    {
        var (request, context, commandBus, mapper, cancellationToken) = requestParameters;

        var command = CreateRestockSubscription.Of(request.CustomerId, request.ProductId, request.Email);

        var result = await commandBus.SendAsync(command, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.CreatedAtRoute(
            new CreateRestockSubscriptionResponse(result.RestockSubscriptionId),
            nameof(GetRestockSubscriptionById),
            new { id = result.RestockSubscriptionId }
        );
    }
}

internal record CreateRestockSubscriptionRequestParameters(
    [FromBody] CreateRestockSubscriptionRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<CreateRestockSubscriptionRequest>;

public record CreateRestockSubscriptionResponse(long RestockSubscriptionId);

public record CreateRestockSubscriptionRequest(long CustomerId, long ProductId, string Email);
