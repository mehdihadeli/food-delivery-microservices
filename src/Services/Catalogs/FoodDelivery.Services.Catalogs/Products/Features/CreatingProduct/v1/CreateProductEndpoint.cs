using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;

// POST api/v1/catalog/products
internal static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // return endpoints.MapCommandEndpoint<
        //     CreateProductRequest,
        //     CreateProductResponse,
        //     CreateProduct,
        //     CreateProductResult
        // >("/", StatusCodes.Status201Created, getId: response => response.Id);

        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        // https://github.com/dotnet/aspnetcore/issues/45871
        return endpoints
            .MapPost("/", Handle)
            .WithTags(ProductsConfigs.Tag)
            .RequireAuthorization()
            .WithName(nameof(CreateProduct))
            .WithDisplayName(nameof(CreateProduct).Humanize())
            .WithSummaryAndDescription(nameof(CreateProduct).Humanize(), nameof(CreateProduct).Humanize())
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?#typedresults-vs-results
            // .Produces<CreateProductResponse>("Product created successfully.", StatusCodes.Status201Created)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<
            Results<CreatedAtRoute<CreateProductResponse>, UnAuthorizedHttpProblemResult, ValidationProblem>
        > Handle([AsParameters] CreateProductRequestParameters requestParameters)
        {
            var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;

            var command = mapper.Map<CreateProduct>(request);

            var result = await commandProcessor.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.CreatedAtRoute(
                new CreateProductResponse(result.Id),
                nameof(GetProductById),
                new { id = result.Id }
            );
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record CreateProductRequestParameters(
    [FromBody] CreateProductRequest Request,
    HttpContext HttpContext,
    ICommandProcessor CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<CreateProductRequest>;

internal record CreateProductResponse(long Id);

// parameters can be pass as null from the user
internal record CreateProductRequest(
    string? Name,
    decimal Price,
    int Stock,
    int RestockThreshold,
    int MaxStockThreshold,
    int Height,
    int Width,
    int Depth,
    string? Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null,
    ProductColor Color = ProductColor.Black,
    ProductStatus Status = ProductStatus.Available,
    ProductType ProductType = ProductType.Food,
    IEnumerable<CreateProductImageRequest>? Images = null
);

internal record CreateProductImageRequest(string ImageUrl, bool IsMain);
