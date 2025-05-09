using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.ProblemDetail.HttpResults;
using Cassandra.Mapping;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Shared;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.v1;

// PUT api/v1/catalog/products/{id}
public static class UpdateProductEndpoint
{
    internal static RouteHandlerBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // return endpoints.MapCommandEndpoint<UpdateProductRequest, UpdateProduct>("/");
        return endpoints
            .MapPost("/{id}", Handle)
            .WithTags(ProductsConfigurations.Tag)
            .RequireAuthorization(policyNames: [Permissions.CatalogsWrite])
            .WithName(nameof(UpdateProduct))
            .WithDisplayName(nameof(UpdateProduct).Humanize())
            .WithSummary(nameof(UpdateProduct).Humanize())
            .WithDescription(nameof(UpdateProduct).Humanize())
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces("Product updated successfully.", StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<Results<NoContent, UnAuthorizedHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] UpdateProductRequestParameters requestParameters
        )
        {
            var (request, id, context, commandBus, cancellationToken) = requestParameters;

            var command = new UpdateProduct(
                ProductId.Of(id),
                Name.Of(request.Name),
                Price.Of(request.Price),
                Stock.Of(request.Stock, request.RestockThreshold, request.MaxStockThreshold),
                request.Status,
                request.ProductType,
                Dimensions.Of(request.Width, request.Height, request.Depth),
                Size.Of(request.Size),
                request.ProductColor,
                CategoryId.Of(request.CategoryId),
                SupplierId.Of(request.SupplierId),
                BrandId.Of(request.BrandId),
                request.Description
            );

            await commandBus.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.NoContent();
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record UpdateProductRequestParameters(
    [FromBody] UpdateProductRequest Request,
    [FromRoute] long Id,
    HttpContext HttpContext,
    ICommandBus CommandBus,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateProductRequest>;

// parameters can be pass as null from the user
public record UpdateProductRequest(
    string Name,
    decimal Price,
    int Stock,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    ProductType ProductType,
    ProductColor ProductColor,
    int Height,
    int Width,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description
);
