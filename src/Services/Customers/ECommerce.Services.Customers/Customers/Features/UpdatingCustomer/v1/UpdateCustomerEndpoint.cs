using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;

internal class UpdateCustomerEndpoint : ICommandMinimalEndpoint<UpdateCustomerRequest>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public async Task<IResult> HandleAsync(
        HttpContext context,
        UpdateCustomerRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(request, nameof(request));

        var command = new UpdateCustomer(
            request.Id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.BirthDate,
            request.Nationality,
            request.Address
        );

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(UpdateCustomerEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("CustomerId", command.Id))
        {
            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        }
    }

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithMetadata(new SwaggerOperationAttribute("Update a Customer", "Updating a Customer"))
            .WithName("UpdateCustomer")
            .WithDisplayName("Update Customer.");
    }
}

internal sealed record UpdateCustomerRequest(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Address = null
);
