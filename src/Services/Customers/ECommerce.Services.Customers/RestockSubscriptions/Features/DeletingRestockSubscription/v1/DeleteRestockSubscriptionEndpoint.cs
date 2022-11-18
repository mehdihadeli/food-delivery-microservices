using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

public class DeleteRestockSubscriptionEndpoint : ICommandMinimalEndpoint<long>
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapDelete("/{id}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscription")
            .WithDisplayName("Delete RestockSubscription for Customer.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        long id,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRestockSubscription(id);

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(DeleteRestockSubscriptionEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("RestockSubscriptionId", command.Id))
        {
            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        }
    }
}
