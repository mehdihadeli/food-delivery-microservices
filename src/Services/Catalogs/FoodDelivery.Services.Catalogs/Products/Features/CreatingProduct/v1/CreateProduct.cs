using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.IdsGenerator;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://enterprisecraftsmanship.com/posts/functional-c-primitive-obsession/

// prevent duplicate validation with using value-objects in command and domain model
public record CreateProduct(
    Name Name,
    Price Price,
    Stock Stock,
    ProductStatus Status,
    ProductType ProductType,
    Dimensions Dimensions,
    Size Size,
    ProductColor Color,
    CategoryId CategoryId,
    SupplierId SupplierId,
    BrandId BrandId,
    string? Description = null,
    IEnumerable<CreateProductRequest.CreateProductImageRequest>? Images = null
) : ITxCommand<CreateProductResult>
{
    public ProductId Id { get; } = ProductId.Of(SnowFlakIdGenerator.NewId());
}

public class CreateProductValidation : AbstractValidator<CreateProduct>
{
    public CreateProductValidation()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.Price).NotNull();
        RuleFor(x => x.Stock).NotNull();
        RuleFor(x => x.Dimensions).NotNull();
        RuleFor(x => x.Size).NotNull();
        RuleFor(x => x.Color).NotNull();
        RuleFor(x => x.CategoryId).NotNull();
        RuleFor(x => x.SupplierId).NotNull();
        RuleFor(x => x.BrandId).NotNull();
    }
}

public class CreateProductHandler(
    ICatalogDbContext catalogDbContext,
    ICategoryChecker categoryChecker,
    IBrandChecker brandChecker,
    ISupplierChecker supplierChecker,
    ILogger<CreateProductHandler> logger
) : ICommandHandler<CreateProduct, CreateProductResult>
{
    public async ValueTask<CreateProductResult> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        var product = command.ToProduct(categoryChecker, brandChecker, supplierChecker);

        // await _domainEventDispatcher.DispatchAsync(cancellationToken, new Events.Domain.CreatingProduct());

        await catalogDbContext.Products.AddAsync(product, cancellationToken: cancellationToken);
        await catalogDbContext.SaveChangesAsync(cancellationToken);

        var created = await catalogDbContext
            .Products.Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .SingleOrDefaultAsync(x => x.Id == product.Id, cancellationToken: cancellationToken);

        logger.LogInformation("Product a with ID: '{ProductId} created.'", created!.Id);

        return new CreateProductResult(created.Id);
    }
}

public record CreateProductResult(long Id);
