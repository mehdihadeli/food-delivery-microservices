using Asp.Versioning.Conventions;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using ECommerce.Services.Customers.Shared;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;

public class DeleteRestockSubscriptionEndpoint : ICommandMinimalEndpoint<long, IResult>
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{{id}}", HandleAsync)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscription")
            .WithDisplayName("Delete RestockSubscription for Customer.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return builder;
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
