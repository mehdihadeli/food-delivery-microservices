using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using ECommerce.Services.Customers.Shared;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription;

public class CreateRestockSubscriptionEndpoint : ICommandMinimalEndpoint<CreateRestockSubscriptionRequest, IResult>
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(RestockSubscriptionsConfigs.RestockSubscriptionsUrl, HandleAsync)
            .AllowAnonymous()
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .Produces<CreateRestockSubscriptionResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("CreateRestockSubscription")
            .WithDisplayName("Register New RestockSubscription for Customer.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return builder;
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        CreateRestockSubscriptionRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new CreateRestockSubscription(request.CustomerId, request.ProductId, request.Email);

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(CreateRestockSubscriptionEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("RestockSubscriptionId", command.Id))
        {
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Created(
                $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{result.RestockSubscription.Id}",
                result);
        }
    }
}
