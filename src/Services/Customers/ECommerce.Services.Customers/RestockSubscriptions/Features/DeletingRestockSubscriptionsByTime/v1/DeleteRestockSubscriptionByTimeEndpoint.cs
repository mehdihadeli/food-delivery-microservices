using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Hellang.Middleware.ProblemDetails;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime.v1;

public class DeleteRestockSubscriptionByTimeEndpoint :
    ICommandMinimalEndpoint<DeleteRestockSubscriptionByTimeRequest>
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapDelete("/", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscriptionByTime")
            .WithDisplayName("Delete RestockSubscriptions by time range.");
    }

    [Authorize(Roles = CustomersConstants.Role.Admin)]
    public async Task<IResult> HandleAsync(
        HttpContext context,
        [FromBody] DeleteRestockSubscriptionByTimeRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
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
