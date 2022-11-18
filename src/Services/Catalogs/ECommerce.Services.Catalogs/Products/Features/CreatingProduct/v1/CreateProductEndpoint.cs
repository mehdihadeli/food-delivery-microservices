using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

// POST api/v1/catalog/products
public static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        return endpoints.MapPost("/", CreateProducts)

            // WithOpenApi should placed before versioning and other things - this fixed in Aps.Versioning.Http 7.0.0-preview.1
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Creating a New Product",
                Description = "Creating a New Product"
            })
            .RequireAuthorization()
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithDisplayName("Create a new product.")

            // .WithMetadata(new SwaggerOperationAttribute("Creating a New Product", "Creating a New Product"))
            // .IsApiVersionNeutral()
            .MapToApiVersion(1.0);
    }

    private static async Task<IResult> CreateProducts(
        CreateProductRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = mapper.Map<CreateProduct>(request);
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(CreateProductEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("ProductId", command.Id))
        {
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.CreatedAtRoute("GetProductById", new {id = result.Product.Id}, result);
        }
    }
}
