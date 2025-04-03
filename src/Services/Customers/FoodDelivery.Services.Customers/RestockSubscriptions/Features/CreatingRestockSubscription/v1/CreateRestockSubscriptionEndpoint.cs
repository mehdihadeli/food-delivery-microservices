using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
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
    public string GroupName => RestockSubscriptionsConfigurations.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigurations.RestockSubscriptionsUrl;
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
            .WithSummary(nameof(CreateRestockSubscription).Humanize())
            .WithDescription(nameof(CreateRestockSubscription).Humanize())
            .WithDisplayName(nameof(GetCustomers).Humanize());
    }

    public async Task<
        Results<CreatedAtRoute<CreateRestockSubscriptionResponse>, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync([AsParameters] CreateRestockSubscriptionRequestParameters requestParameters)
    {
        var (request, context, commandBus, cancellationToken) = requestParameters;

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
    CancellationToken CancellationToken
) : IHttpCommand<CreateRestockSubscriptionRequest>;

public record CreateRestockSubscriptionResponse(long RestockSubscriptionId);

internal record CreateRestockSubscriptionRequest(long CustomerId, long ProductId, string Email);
