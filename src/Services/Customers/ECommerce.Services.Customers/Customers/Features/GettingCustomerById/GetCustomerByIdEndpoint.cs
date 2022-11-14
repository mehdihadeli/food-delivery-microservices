using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using ECommerce.Services.Customers.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerById;

public class GetCustomerByIdEndpointEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(
                $"{CustomersConfigs.CustomersPrefixUri}/{{id}}",
                GetCustomerById)
            // .RequireAuthorization()
            .Produces<GetCustomerByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(CustomersConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Getting a Customer By Id", "Getting a Customer By Id"))
            .WithName("GetCustomerById")
            .WithDisplayName("Get Customer By Id.")
            .WithApiVersionSet(CustomersConfigs.VersionSet)
            .HasApiVersion(1.0);

        return builder;
    }

    private static async Task<IResult> GetCustomerById(
        long id,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetCustomerByIdEndpointEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("CustomerId", id))
        {
            var result = await queryProcessor.SendAsync(new GetCustomerById(id), cancellationToken);

            return Results.Ok(result);
        }
    }
}
