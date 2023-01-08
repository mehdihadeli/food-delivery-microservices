using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Hellang.Middleware.ProblemDetails;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionBySubscriptionId.v1;

public class GetRestockSubscriptionBySubscriptionIdEndpoint : IQueryMinimalEndpoint<long>
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;


    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/{restockSubscriptionId}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces<GetRestockSubscriptionBySubscriptionIdResponse>(StatusCodes.Status200OK)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithName("GetRestockSubscriptionBySubscriptionId")
            .WithOpenApi(operation => new(operation)
            {
                Description = "Getting RestockSubscription By SubscriptionId.",
                Summary = "Getting RestockSubscription By SubscriptionId."
            })
            .WithDisplayName("Get RestockSubscription By SubscriptionId.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        long restockSubscriptionId,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty(
                   "Endpoint",
                   nameof(GetRestockSubscriptionBySubscriptionIdEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("RestockSubscriptionId", restockSubscriptionId))
        {
            var result = await queryProcessor.SendAsync(
                new GetRestockSubscriptionBySubscriptionId(restockSubscriptionId),
                cancellationToken);

            return Results.Ok(result);
        }
    }
}
