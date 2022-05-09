using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace Store.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionById;

public class GetRestockSubscriptionByIdEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(
                $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{{id}}",
                GetRestockSubscriptionById)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces<GetRestockSubscriptionByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetRestockSubscriptionById")
            .WithDisplayName("Get RestockSubscription By Id.");

        return builder;
    }

    private static async Task<IResult> GetRestockSubscriptionById(
        long id,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        var result = await queryProcessor.SendAsync(new GetRestockSubscriptionById(id), cancellationToken);

        return Results.Ok(result);
    }
}
