using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionBySubscriptionId.v1;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GetRestockSubscriptionById;

public class GetRestockSubscriptionByIdEndpoint : IQueryMinimalEndpoint<Guid>
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;


    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/{id:guid}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces<GetRestockSubscriptionByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetRestockSubscriptionById")
            .WithOpenApi(operation => new(operation)
            {
                Description = "Getting RestockSubscription By SubscriptionId.",
                Summary = "Getting RestockSubscription By SubscriptionId."
            })
            .WithDisplayName("Get RestockSubscription By Id.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        Guid id,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty(
                   "Endpoint",
                   nameof(GetRestockSubscriptionBySubscriptionIdEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("Id", id))
        {
            var result = await queryProcessor.SendAsync(
                new GetRestockSubscriptionById(id),
                cancellationToken);

            return Results.Ok(result);
        }
    }
}
