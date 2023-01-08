using System.Globalization;
using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;
using Hellang.Middleware.ProblemDetails;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

// POST api/v1/catalog/products
public static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        // https://github.com/dotnet/aspnetcore/issues/45871
        return endpoints.MapPost("/", CreateProducts)

            // WithOpenApi should placed before versioning and other things - this fixed in Aps.Versioning.Http 7.0.0-preview.1
            .WithOpenApi(operation =>
            {
                operation.Summary = "Creating a New Product";
                operation.Description = "Creating a New Product";
                operation.Responses[StatusCodes.Status401Unauthorized.ToString(CultureInfo.InvariantCulture)]
                    .Description = "UnAuthorized request.";
                operation.Responses[StatusCodes.Status400BadRequest.ToString(CultureInfo.InvariantCulture)]
                    .Description = "Invalid input for creating product.";
                operation.Responses[StatusCodes.Status201Created.ToString(CultureInfo.InvariantCulture)].Description =
                    "Product created successfully.";

                return operation;
            })
            //.RequireAuthorization()
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithDisplayName("Create a new product.")
            .MapToApiVersion(1.0);

        // .WithMetadata(new SwaggerResponseAttribute(
        //     StatusCodes.Status401Unauthorized,
        //     "UnAuthorized request.",
        //     typeof(StatusCodeProblemDetails)))
        // .WithMetadata(new SwaggerResponseAttribute(
        //     StatusCodes.Status400BadRequest,
        //     "Invalid input for creating product.",
        //     typeof(StatusCodeProblemDetails)))
        // .WithMetadata(
        //     new SwaggerResponseAttribute(
        //         StatusCodes.Status201Created,
        //         "Product created successfully.",
        //         typeof(CreateProductResponse)))
        // .WithMetadata(new SwaggerOperationAttribute("Creating a New Product", "Creating a New Product"))
        // .IsApiVersionNeutral()
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
