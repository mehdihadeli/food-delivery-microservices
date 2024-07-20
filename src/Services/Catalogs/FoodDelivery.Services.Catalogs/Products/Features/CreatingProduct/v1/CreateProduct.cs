using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://enterprisecraftsmanship.com/posts/functional-c-primitive-obsession/

// prevent duplicate validation with using value-objects in command and domain model
internal record CreateProduct(
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
    IEnumerable<CreateProductImageRequest>? Images = null
) : ITxCreateCommand<CreateProductResult>
{
    public long Id { get; } = SnowFlakIdGenerator.NewId();

    /// <summary>
    /// Create product with in-line validation.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="price"></param>
    /// <param name="stock"></param>
    /// <param name="restockThreshold"></param>
    /// <param name="maxStockThreshold"></param>
    /// <param name="status"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    /// <param name="productType"></param>
    /// <param name="categoryId"></param>
    /// <param name="supplierId"></param>
    /// <param name="brandId"></param>
    /// <param name="description"></param>
    /// <param name="images"></param>
    /// <returns></returns>
    public static CreateProduct Of(
        string? name,
        decimal price,
        int stock,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        int width,
        int height,
        int depth,
        string? size,
        ProductColor color,
        ProductType productType,
        long categoryId,
        long supplierId,
        long brandId,
        string? description = null,
        IEnumerable<CreateProductImageRequest>? images = null
    )
    {
        // Or we can use FluentValidation like `new CreateProductValidator()!.ValidateAndThrow(value);` for validating input here
        return new CreateProduct(
            Name.Of(name),
            Price.Of(price),
            Stock.Of(stock, restockThreshold, maxStockThreshold),
            status.NotBeEmpty(),
            productType,
            Dimensions.Of(width, height, depth),
            Size.Of(size),
            color.NotBeEmpty(),
            CategoryId.Of(categoryId),
            SupplierId.Of(supplierId),
            BrandId.Of(brandId),
            description,
            images
        );
    }
}

internal class CreateProductHandler : ICommandHandler<CreateProduct, CreateProductResult>
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICategoryChecker _categoryChecker;
    private readonly IBrandChecker _brandChecker;
    private readonly ISupplierChecker _supplierChecker;
    private readonly ICatalogDbContext _catalogDbContext;

    public CreateProductHandler(
        ICatalogDbContext catalogDbContext,
        IMapper mapper,
        ICategoryChecker categoryChecker,
        IBrandChecker brandChecker,
        ISupplierChecker supplierChecker,
        ILogger<CreateProductHandler> logger
    )
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
        _categoryChecker = categoryChecker;
        _brandChecker = brandChecker;
        _supplierChecker = supplierChecker;
        _logger = logger;
    }

    public async Task<CreateProductResult> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (
            name,
            price,
            stock,
            status,
            type,
            dimensions,
            size,
            color,
            categoryId,
            supplierId,
            brandId,
            description,
            imageItems
        ) = command;

        var images = imageItems
            ?.Select(
                x =>
                    new ProductImage(
                        EntityId.Of(SnowFlakIdGenerator.NewId()),
                        x.ImageUrl,
                        x.IsMain,
                        ProductId.Of(command.Id)
                    )
            )
            .ToList();

        // await _domainEventDispatcher.DispatchAsync(cancellationToken, new Events.Domain.CreatingProduct());

        // orchestration on multiple aggregate and entities in application service or handlers
        var product = Product.Create(
            ProductId.Of(command.Id),
            name,
            null,
            stock,
            status,
            type,
            dimensions,
            size,
            color,
            description,
            price,
            categoryId,
            supplierId,
            brandId,
            async cid => await _catalogDbContext.CategoryExistsAsync(cid!, cancellationToken: cancellationToken),
            _supplierChecker,
            _brandChecker,
            images
        );

        await _catalogDbContext.Products.AddAsync(product, cancellationToken: cancellationToken);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        var created = await _catalogDbContext.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .SingleOrDefaultAsync(x => x.Id == product.Id, cancellationToken: cancellationToken);

        _logger.LogInformation("Product a with ID: '{ProductId} created.'", created!.Id);

        return new CreateProductResult(created.Id);
    }
}

internal record CreateProductResult(long Id);
