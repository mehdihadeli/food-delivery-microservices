using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Shared;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime.v1;

internal class DeleteRestockSubscriptionByTimeEndpoint
    : ICommandMinimalEndpoint<
        DeleteRestockSubscriptionByTimeRequest,
        DeleteRestockSubscriptionByTimeRequestParameters,
        UnAuthorizedHttpProblemResult,
        NotFoundHttpProblemResult,
        NoContent,
        ValidationProblem
    >
{
    public string GroupName => RestockSubscriptionsConfigurations.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigurations.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapDelete("/", HandleAsync)
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesValidationProblem()
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Role.Admin)
            .WithName(nameof(DeleteRestockSubscriptionsByTime))
            .WithName(nameof(DeleteRestockSubscriptionsByTime))
            .WithDisplayName(nameof(DeleteRestockSubscriptionsByTime).Humanize())
            .WithSummary(nameof(DeleteRestockSubscriptionsByTime).Humanize())
            .WithDescription(nameof(DeleteRestockSubscriptionsByTime).Humanize());
    }

    public async Task<
        Results<UnAuthorizedHttpProblemResult, NotFoundHttpProblemResult, NoContent, ValidationProblem>
    > HandleAsync([AsParameters] DeleteRestockSubscriptionByTimeRequestParameters requestParameters)
    {
        var (request, context, commandBus, cancellationToken) = requestParameters;

        var command = new DeleteRestockSubscriptionsByTime(request.From, request.To);

        await commandBus.SendAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }
}

internal record DeleteRestockSubscriptionByTimeRequestParameters(
    [FromBody] DeleteRestockSubscriptionByTimeRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<DeleteRestockSubscriptionByTimeRequest>;

internal record DeleteRestockSubscriptionByTimeRequest(DateTime? From = null, DateTime? To = null);
