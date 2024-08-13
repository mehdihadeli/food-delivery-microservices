using System.Collections.Immutable;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Dtos.V1;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingMaxThreshold.V1;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.V1.Events.Domain;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.V1.Events;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingProductPrice.V1;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingProductSupplier.V1.Events;
using FoodDelivery.Services.Catalogs.Products.Features.ChangingRestockThreshold.V1;
using FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.V1.Events.Domain;
using FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.V1.Events.Domain;
using FoodDelivery.Services.Catalogs.Products.Features.ReplenishingProductStock.V1.Events.Domain;
using FoodDelivery.Services.Catalogs.Products.Rules;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;

namespace FoodDelivery.Services.Catalogs.Products.Models;

// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://enterprisecraftsmanship.com/posts/link-to-an-aggregate-reference-or-id/
// https://ardalis.com/avoid-collections-as-properties/?utm_sq=grcpqjyka3
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public class Product : Aggregate<ProductId>
{
    private List<ProductImage> _images = new();

    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Product() { }

    public Name Name { get; private set; } = default!;
    public ProductType ProductType { get; private set; }
    public string? Description { get; private set; }
    public Price Price { get; private set; } = default!;
    public ProductInformation ProductInformation { get; private set; }
    public ProductColor Color { get; private set; }
    public ProductStatus ProductStatus { get; private set; }
    public Category? Category { get; private set; } = default!;
    public CategoryId CategoryId { get; private set; } = default!;
    public SupplierId SupplierId { get; private set; } = default!;
    public Supplier? Supplier { get; private set; }
    public BrandId BrandId { get; private set; } = default!;
    public Brand? Brand { get; private set; }
    public Size Size { get; private set; } = default!;
    public Stock Stock { get; set; } = default!;
    public Dimensions Dimensions { get; private set; } = default!;
    public IReadOnlyList<ProductImage>? Images => _images;

    // https://github.com/ardalis/DDD-NoDuplicates
    // https://stackoverflow.com/questions/66289815/dependent-entities-within-same-aggregate/66299403
    // https://stackoverflow.com/questions/66330442/ddd-and-cqrs-how-to-make-sure-if-provided-relation-id-really-exists-as-another?noredirect=1&lq=1
    // https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Registrations/Domain/UserRegistrations/UserRegistration.cs#L52
    // https://enterprisecraftsmanship.com/posts/domain-vs-application-services/
    // https://event-driven.io/en/how_to_validate_business_logic/
    // https://www.dandoescode.com/blog/domain-driven-design-patterns-for-aggregate-creation-mastery
    // https://www.kamilgrzybek.com/blog/posts/domain-model-validation
    public static Product Create(
        ProductId? id,
        Name? name,
        ProductInformation? productInformation,
        Stock? stock,
        ProductStatus status,
        ProductType type,
        Dimensions? dimensions,
        Size? size,
        ProductColor color,
        string? description,
        Price? price,
        CategoryId? categoryId,
        SupplierId? supplierId,
        BrandId? brandId,
        AggregateFuncOperation<CategoryId?, bool>? categoryChecker,
        ISupplierChecker? supplierChecker,
        IBrandChecker? brandChecker,
        IList<ProductImage>? images = null
    )
    {
        // input validation will do in the `command` and our `value objects` before arriving to entity and makes or domain cleaner, here we just do business validation
        var product = new Product { Id = id.NotBeNull(), Stock = stock.NotBeNull() };

        product.ChangeProductDetail(name, status, type, dimensions, size, color, productInformation, description);

        product.ChangePrice(price);
        product.AddProductImages(images);
        product.ChangeCategory(categoryChecker, categoryId);
        product.ChangeBrand(brandChecker, brandId);
        product.ChangeSupplier(supplierChecker, supplierId);

        // here we do not use auto-mapping because we want to validate the data
        product.AddDomainEvents(
            ProductCreated.Of(
                product.Id,
                product.Name,
                product.Price,
                product.Stock,
                product.ProductStatus,
                product.Dimensions,
                product.Size,
                product.Color,
                product.CategoryId,
                product.SupplierId,
                product.BrandId,
                DateTime.Now,
                product.Description,
                product.Images?.Select(x => new ProductImageDto(x.Id, x.ProductId, x.ImageUrl, x.IsMain))
            )
        );

        return product;
    }

    // https://event-driven.io/en/property-sourcing/
    // https://stackoverflow.com/questions/59558931/should-there-be-an-update-event-per-property-or-an-update-event-per-entity-with
    public void ChangeProductDetail(
        Name? name,
        ProductStatus status,
        ProductType productType,
        Dimensions? dimensions,
        Size? size,
        ProductColor color,
        ProductInformation? productInformation,
        string? description
    )
    {
        // input validation will do in the command and our value objects, here we just do business validation
        name.NotBeNull();
        Name = name;

        status.NotBeEmpty();
        ProductStatus = status;

        productType.NotBeEmpty();
        ProductType = productType;

        dimensions.NotBeNull();
        Dimensions = dimensions;

        size.NotBeNull();
        Size = size;

        color.NotBeEmpty();
        Color = color;

        // input validation will do in the command and our value objects, here we just do business validation
        productInformation.NotBeNull();
        ProductInformation = productInformation;

        Description = description;

        // ProductDetail Changed event
    }

    /// <summary>
    /// Change product price.
    /// </summary>
    /// <remarks>
    /// Raise a <see cref="ProductPriceChanged"/>.
    /// </remarks>
    /// <param name="price">The price to be changed.</param>
    public void ChangePrice(Price? price)
    {
        price.NotBeNull();
        if (Price == price)
            return;

        Price = price;

        AddDomainEvents(ProductPriceChanged.Of(price));
    }

    /// <summary>
    /// Decrements the quantity of a particular item in inventory and ensures the restockThreshold hasn't
    /// been breached. If so, a RestockRequest is generated in CheckThreshold.
    /// </summary>
    /// <param name="quantity">The number of items to debit.</param>
    /// <returns>int: Returns the number actually removed from stock. </returns>
    public int DebitStock(int quantity)
    {
        if (quantity < 0)
            quantity *= -1;

        if (HasStock(quantity) == false)
        {
            throw new InsufficientStockException(
                $"Empty stock, product item '{Name}' with quantity '{quantity}' is not available."
            );
        }

        var (available, restockThreshold, maxStockThreshold) = Stock;

        int removed = Math.Min(quantity, available);

        Stock = Stock.Of(available - removed, restockThreshold, maxStockThreshold);

        var (newAvailable, newRestockThreshold, newMaxStockThreshold) = Stock;

        if (newAvailable <= newRestockThreshold)
        {
            AddDomainEvents(
                ProductRestockThresholdReached.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity)
            );
        }

        AddDomainEvents(ProductStockDebited.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity));

        return removed;
    }

    /// <summary>
    /// Increments the quantity of a particular item in inventory.
    /// </summary>
    /// <returns>int: Returns the quantity that has been added to stock.</returns>
    /// <param name="quantity">The number of items to Replenish.</param>
    public Stock ReplenishStock(int quantity)
    {
        var (available, restockThreshold, maxStockThreshold) = Stock;

        // we don't have enough space in the inventory
        if (available + quantity > maxStockThreshold)
        {
            throw new MaxStockThresholdReachedException(
                $"Max stock threshold has been reached. Max stock threshold is {maxStockThreshold}"
            );
        }

        Stock = Stock.Of(available + quantity, restockThreshold, maxStockThreshold);

        var (newAvailable, newRestockThreshold, newMaxStockThreshold) = Stock;

        AddDomainEvents(
            ProductStockReplenished.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity)
        );

        return Stock;
    }

    public Stock ChangeMaxStockThreshold(int newMaxStockThreshold)
    {
        var (available, restockThreshold, maxStockThreshold) = Stock;
        Stock = Stock.Of(available, restockThreshold, maxStockThreshold);

        AddDomainEvents(MaxThresholdChanged.Of(Id, maxStockThreshold));

        return Stock;
    }

    public Stock ChangeRestockThreshold(int restockThreshold)
    {
        Stock = Stock.Of(Stock.Available, restockThreshold, Stock.MaxStockThreshold);

        AddDomainEvents(RestockThresholdChanged.Of(Id, restockThreshold));

        return Stock;
    }

    public bool HasStock(int quantity)
    {
        return Stock.Available >= quantity;
    }

    public void Activate() => ProductStatus = ProductStatus.Available;

    public void DeActive() => ProductStatus = ProductStatus.Unavailable;

    // passing delegate like a domain service

    /// <summary>
    /// Sets category.
    /// </summary>
    /// <param name="categoryChecker">The checker for CategoryId.</param>
    /// <param name="categoryId">The categoryId to be changed.</param>
    public void ChangeCategory(AggregateFuncOperation<CategoryId?, bool>? categoryChecker, CategoryId? categoryId)
    {
        CheckRule(new CategoryIdShouldExistRuleWithExceptionType(categoryChecker, categoryId));

        CategoryId = categoryId;

        // add event to domain events list that will be raise during committing transaction
        AddDomainEvents(ProductCategoryChanged.Of(categoryId, Id));
    }

    /// <summary>
    /// Sets supplier.
    /// </summary>
    /// <param name="supplierChecker">The supplier checker.</param>
    /// <param name="supplierId">The supplierId to be changed.</param>
    public void ChangeSupplier(ISupplierChecker? supplierChecker, SupplierId? supplierId)
    {
        CheckRule(new SupplierShouldExistRule(supplierChecker, supplierId));

        SupplierId = supplierId;

        AddDomainEvents(ProductSupplierChanged.Of(supplierId, Id));
    }

    /// <summary>
    ///  Sets brand.
    /// </summary>
    /// <param name="brandChecker">The brand checker.</param>
    /// <param name="brandId">The brandId to be changed.</param>
    public void ChangeBrand(IBrandChecker? brandChecker, BrandId? brandId)
    {
        CheckRule(new BrandIdShouldExistRuleWithExceptionType(brandChecker, brandId));

        BrandId = brandId;

        AddDomainEvents(ProductBrandChanged.Of(brandId, Id));
    }

    public void AddProductImages(IList<ProductImage>? productImages)
    {
        if (productImages is null)
        {
            _images = null!;
            return;
        }

        _images.AddRange(productImages);
    }

    public void Deconstruct(
        out long id,
        out string name,
        out int availableStock,
        out int restockThreshold,
        out int maxStockThreshold,
        out ProductStatus status,
        out int width,
        out int height,
        out int depth,
        out string size,
        out ProductColor color,
        out string? description,
        out decimal price,
        out long categoryId,
        out long supplierId,
        out long brandId,
        out IList<ProductImage>? images
    ) =>
        (
            id,
            name,
            availableStock,
            restockThreshold,
            maxStockThreshold,
            status,
            width,
            height,
            depth,
            size,
            color,
            description,
            price,
            categoryId,
            supplierId,
            brandId,
            images
        ) = (
            Id,
            Name,
            Stock.Available,
            Stock.RestockThreshold,
            Stock.MaxStockThreshold,
            ProductStatus,
            Dimensions.Width,
            Dimensions.Height,
            Dimensions.Depth,
            Size,
            Color,
            Description,
            Price.Value,
            CategoryId,
            SupplierId,
            BrandId,
            Images?.ToImmutableList()
        );
}
