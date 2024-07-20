using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
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
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            .RequireAuthorization()
            .WithName(nameof(CreateCustomer))
            .WithDisplayName(nameof(CreateCustomer).Humanize())
            .WithSummaryAndDescription(nameof(CreateCustomer).Humanize(), nameof(CreateCustomer).Humanize());
        // .Produces<CreateCustomerRequest>("Customer created successfully.", StatusCodes.Status201Created)
        // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
    }

    public async Task<
        Results<CreatedAtRoute<CreateCustomerResponse>, UnAuthorizedHttpProblemResult, ValidationProblem>
    > HandleAsync(CreateCustomerRequestParameters requestParameters)
    {
        var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;

        var command = CreateCustomer.Of(request.Email);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.CreatedAtRoute(
            new CreateCustomerResponse(result.CustomerId),
            nameof(GettingCustomerById),
            new { id = result.CustomerId }
        );
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record CreateCustomerRequestParameters(
    [FromBody] CreateCustomerRequest Request,
    HttpContext HttpContext,
    ICommandProcessor CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<CreateCustomerRequest>;

public record CreateCustomerRequest(string? Email);

public record CreateCustomerResponse(long CustomerId);
