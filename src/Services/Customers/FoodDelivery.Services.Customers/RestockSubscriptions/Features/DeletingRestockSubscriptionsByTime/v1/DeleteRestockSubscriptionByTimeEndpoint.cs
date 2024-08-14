using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
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
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapDelete("/", HandleAsync)
            // .Produces(StatusCodes.Status204NoContent)
            // .ProducesValidationProblem()
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .WithName(nameof(DeleteRestockSubscriptionsByTime))
            .WithName(nameof(DeleteRestockSubscriptionsByTime))
            .WithDisplayName(nameof(DeleteRestockSubscriptionsByTime).Humanize())
            .WithSummaryAndDescription(
                nameof(DeleteRestockSubscriptionsByTime).Humanize(),
                nameof(DeleteRestockSubscriptionsByTime).Humanize()
            );
    }

    public async Task<
        Results<UnAuthorizedHttpProblemResult, NotFoundHttpProblemResult, NoContent, ValidationProblem>
    > HandleAsync([AsParameters] DeleteRestockSubscriptionByTimeRequestParameters requestParameters)
    {
        var (request, context, commandBus, mapper, cancellationToken) = requestParameters;

        var command = new DeleteRestockSubscriptionsByTime(request.From, request.To);

        await commandBus.SendAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }
}

internal record DeleteRestockSubscriptionByTimeRequestParameters(
    [FromBody] DeleteRestockSubscriptionByTimeRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<DeleteRestockSubscriptionByTimeRequest>;

internal record DeleteRestockSubscriptionByTimeRequest(DateTime? From = null, DateTime? To = null);
