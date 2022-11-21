using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;

public class CreateCustomerEndpoint : ICommandMinimalEndpoint<CreateCustomerRequest>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/", HandleAsync)
            .AllowAnonymous()
            .Produces<CreateCustomerResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithMetadata(new SwaggerOperationAttribute("Creating a Customer", "Creating a Customer"))
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        CreateCustomerRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new CreateCustomer(request.Email);

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(CreateCustomerEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("CustomerId", command.Id))
        {
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Created($"{CustomersConfigs.CustomersPrefixUri}/{result.CustomerId}", result);
        }
    }
}
