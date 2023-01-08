using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProductById.v1;

// GET api/v1/catalog/products/{id}
public static class GetProductByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{id}", GetProductById)
            // .RequireAuthorization()
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .WithMetadata(new SwaggerOperationAttribute("Getting Product by InternalCommandId", "Getting Product by InternalCommandId"))
            .WithName("GetProductById")
            .WithDisplayName("Get product By InternalCommandId.");
    }

    private static async Task<IResult> GetProductById(
        long id,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetProductByIdEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", id))
        {
            var result = await queryProcessor.SendAsync(new GetProductById(id), cancellationToken);

            return Results.Ok(result);
        }
    }
}
