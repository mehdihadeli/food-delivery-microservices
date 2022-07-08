using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using SerilogTimings;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime;

public class DeleteRestockSubscriptionByTimeEndpoint : IMinimalEndpointConfiguration
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}", DeleteRestockSubscriptionByTime)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscriptionByTime")
            .WithDisplayName("Delete RestockSubscriptions by time range.");

        return builder;
    }

    [Authorize(Roles = CustomersConstants.Role.Admin)]
    private static async Task<IResult> DeleteRestockSubscriptionByTime(
        [FromBody] DeleteRestockSubscriptionByTimeRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new DeleteRestockSubscriptionsByTime(request.From, request.To);

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(DeleteRestockSubscriptionByTimeEndpoint)))
        {
            await commandProcessor.SendAsync(command, cancellationToken);
        }

        return Results.NoContent();
    }
}
