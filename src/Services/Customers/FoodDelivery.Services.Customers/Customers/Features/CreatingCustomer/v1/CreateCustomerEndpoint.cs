using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;

internal class CreateCustomerEndpoint
    : ICommandMinimalEndpoint<
        CreateCustomerRequest,
        CreateCustomerRequestParameters,
        CreatedAtRoute<CreateCustomerResponse>,
        UnAuthorizedHttpProblemResult,
        ValidationProblem
    >
{
    public string GroupName => CustomersConfigurations.Tag;
    public string PrefixRoute => CustomersConfigurations.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            //.RequireAuthorization()
            .WithName(nameof(CreateCustomer))
            .WithDisplayName(nameof(CreateCustomer).Humanize())
            .WithSummary(nameof(CreateCustomer).Humanize())
            .WithDescription(nameof(CreateCustomer).Humanize());
        // .Produces<CreateCustomerRequest>("CustomerReadModel created successfully.", StatusCodes.Status201Created)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<CreatedAtRoute<CreateCustomerResponse>, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync([AsParameters] CreateCustomerRequestParameters requestParameters)
    {
        var (request, context, commandBus, cancellationToken) = requestParameters;

        var command = CreateCustomer.Of(request.Email);

        var result = await commandBus.SendAsync(command, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.CreatedAtRoute(
            new CreateCustomerResponse(result.CustomerId),
            nameof(GetCustomerByCustomerId),
            new { customerId = result.CustomerId }
        );
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record CreateCustomerRequestParameters(
    [FromBody] CreateCustomerRequest Request,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<CreateCustomerRequest>;

internal record CreateCustomerRequest(string? Email);

public record CreateCustomerResponse(long CustomerId);
