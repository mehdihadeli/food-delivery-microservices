using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

internal class DeleteRestockSubscriptionEndpoint : IMinimalEndpoint
{
    public string GroupName => RestockSubscriptionsConfigurations.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigurations.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public async Task<
        Results<NoContent, NotFoundHttpProblemResult, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync([AsParameters] DeleteRestockSubscriptionRequestParameters requestParameters)
    {
        var (id, context, commandBus, cancellationToken) = requestParameters;

        var command = DeleteRestockSubscription.Of(id);

        await commandBus.SendAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapDelete("/{id}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesValidationProblem()
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(DeleteRestockSubscription))
            .WithDescription(nameof(DeleteRestockSubscription).Humanize())
            .WithSummary(nameof(DeleteRestockSubscription).Humanize())
            .WithDisplayName(nameof(DeleteRestockSubscription).Humanize());
    }
}

internal record DeleteRestockSubscriptionRequestParameters(
    [FromRoute] long Id,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
);
