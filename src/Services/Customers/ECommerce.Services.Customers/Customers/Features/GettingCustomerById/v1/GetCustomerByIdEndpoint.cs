using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdEndpointEndpoint : IQueryMinimalEndpoint<Guid>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/{id:guid}", HandleAsync)
            // .RequireAuthorization()
            .Produces<GetCustomerByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithMetadata(new SwaggerOperationAttribute("Getting a Customer By InternalCommandId", "Getting a Customer By InternalCommandId"))
            .WithName("GetCustomerById")
            .WithDisplayName("Get Customer By InternalCommandId.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        Guid id,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetCustomerByIdEndpointEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("InternalCommandId", id))
        {
            var result = await queryProcessor.SendAsync(new GetCustomerById(id), cancellationToken);

            return Results.Ok(result);
        }
    }
}
