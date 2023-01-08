using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using ECommerce.Services.Customers.Customers.Features.GettingCustomerById.v1;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;

public class GetCustomerByCustomerIdEndpointEndpoint : IQueryMinimalEndpoint<long>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/{customerId}", HandleAsync)
            // .RequireAuthorization()
            .Produces<GetCustomerByCustomerIdResponse>(StatusCodes.Status200OK)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithMetadata(new SwaggerOperationAttribute(
                "Getting a Customer By CustomerId",
                "Getting a Customer By CustomerId"))
            .WithName("GetCustomerByCustomerId")
            .WithDisplayName("Get Customer By CustomerId.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        long customerId,
        IQueryProcessor queryProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetCustomerByIdEndpointEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("CustomerId", customerId))
        {
            var result = await queryProcessor.SendAsync(new GetCustomerByCustomerId(customerId), cancellationToken);

            return Results.Ok(result);
        }
    }
}
