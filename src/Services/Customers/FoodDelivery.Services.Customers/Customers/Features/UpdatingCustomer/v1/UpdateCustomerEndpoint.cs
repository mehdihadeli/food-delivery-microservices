using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using Humanizer;

namespace FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;

internal class UpdateCustomerEndpoint : ICommandMinimalEndpoint<UpdateCustomerRequest, UpdateCustomerRequestParameters>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPut("/{id}", HandleAsync)
            .RequireAuthorization()
            .RequireAuthorization()
            .WithName(nameof(UpdateCustomer))
            .WithDisplayName(nameof(UpdateCustomer).Humanize())
            .WithSummaryAndDescription(nameof(UpdateCustomer).Humanize(), nameof(UpdateCustomer).Humanize());

        // .Produces("Customer updated successfully.", StatusCodes.Status204NoContent)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
    }

    public async Task<IResult> HandleAsync(UpdateCustomerRequestParameters requestParameters)
    {
        var (request, id, context, commandBus, cancellationToken) = requestParameters;

        var command = request.ToUpdateCustomer();
        command = command with { Id = id };

        await commandBus.SendAsync(command, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.NoContent();
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record UpdateCustomerRequestParameters(
    [FromBody] UpdateCustomerRequest Request,
    [FromRoute] long Id,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateCustomerRequest>;

// These parameters can be pass null from the user
internal sealed record UpdateCustomerRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Address = null
);
