using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.Requests;
using ECommerce.Services.Catalogs.Shared;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct;

// POST api/v1/catalog/products
public static class CreateProductEndpoint
{
    internal static IEndpointRouteBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ProductsConfigs.ProductsPrefixUri}", CreateProducts)
            .WithTags(ProductsConfigs.Tag)
            .RequireAuthorization()
            .Produces<CreateProductResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithDisplayName("Create a new product.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)

            // .IsApiVersionNeutral()
            // .MapToApiVersion(1.0)
            .HasApiVersion(1.0);

        return endpoints;
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
