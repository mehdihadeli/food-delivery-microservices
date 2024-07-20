using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Catalogs.Products.Models;
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
            .WithTags(ProductsConfigs.Tag)
            .RequireAuthorization()
            .WithName(nameof(UpdateProduct))
            .WithDisplayName(nameof(UpdateProduct).Humanize())
            .WithSummaryAndDescription(nameof(UpdateProduct).Humanize(), nameof(UpdateProduct).Humanize())
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces("Product updated successfully.", StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<Results<NoContent, UnAuthorizedHttpProblemResult, ValidationProblem>> Handle(
            [AsParameters] UpdateProductRequestParameters requestParameters
        )
        {
            var (request, id, context, commandProcessor, mapper, cancellationToken) = requestParameters;

            var command = mapper.Map<UpdateProduct>(request);
            command = command with { Id = id };

            await commandProcessor.SendAsync(command, cancellationToken);

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
    ICommandProcessor CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateProductRequest>;

// parameters can be pass as null from the user
public record UpdateProductRequest(
    long Id,
    string? Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    ProductType ProductType,
    ProductColor ProductColor,
    int Height,
    int Width,
    int Depth,
    string? Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description
);
