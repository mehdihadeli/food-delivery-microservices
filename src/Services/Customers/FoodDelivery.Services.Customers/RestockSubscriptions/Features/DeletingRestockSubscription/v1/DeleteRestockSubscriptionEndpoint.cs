using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

internal class DeleteRestockSubscriptionEndpoint : IMinimalEndpoint
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public async Task<
        Results<NoContent, NotFoundHttpProblemResult, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync([AsParameters] DeleteRestockSubscriptionRequestParameters requestParameters)
    {
        var (id, context, commandBus, mapper, cancellationToken) = requestParameters;

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
            .WithSummaryAndDescription(
                nameof(DeleteRestockSubscription).Humanize(),
                nameof(DeleteRestockSubscription).Humanize()
            )
            .WithDisplayName(nameof(DeleteRestockSubscription).Humanize());
    }
}

internal record DeleteRestockSubscriptionRequestParameters(
    [FromRoute] long Id,
    HttpContext HttpContext,
    ICommandBus commandBus,
    IMapper Mapper,
    CancellationToken CancellationToken
);
