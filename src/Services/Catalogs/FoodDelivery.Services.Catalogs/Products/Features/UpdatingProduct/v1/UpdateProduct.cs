using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Categories.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Application;
using Mediator;

namespace FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.v1;

public record UpdateProduct(
    ProductId Id,
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
    string? Description = null
) : ITxCommand;

public class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
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

public class UpdateProductInvalidateCache : InvalidateCacheRequest<UpdateProduct>
{
    public override IEnumerable<string> CacheKeys(UpdateProduct request)
    {
        yield return $"{Prefix}{nameof(GetProductById)}_{request.Id}";
    }
}

public class UpdateProductCommandHandler(
    ICatalogDbContext catalogDbContext,
    ICategoryChecker categoryChecker,
    IBrandChecker brandChecker,
    ISupplierChecker supplierChecker
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<UpdateProduct>
{
    public async ValueTask<Unit> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (
            id,
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
            description
        ) = command;

        var product = await catalogDbContext.FindProductByIdAsync(id);
        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        var category = await catalogDbContext.FindCategoryAsync(categoryId);
        if (category is null)
            throw new CategoryNotFoundException(categoryId);

        var brand = await catalogDbContext.FindBrandAsync(brandId);
        if (brand is null)
            throw new BrandNotFoundException(brandId);

        var supplier = await catalogDbContext.FindSupplierByIdAsync(supplierId);
        if (supplier is null)
            throw new SupplierNotFoundException(supplierId);

        product.ChangeCategory(categoryChecker, CategoryId.Of(categoryId));
        product.ChangeBrand(brandChecker, BrandId.Of(brandId));
        product.ChangeSupplier(supplierChecker, SupplierId.Of(supplierId));

        product.ChangeProductDetail(
            name,
            status,
            type,
            dimensions,
            size,
            color,
            ProductInformation.Of(name.Value, name.Value),
            description
        );
        product.ChangePrice(price);
        product.ChangeMaxStockThreshold(stock.MaxStockThreshold);
        product.ChangeRestockThreshold(stock.RestockThreshold);

        await catalogDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
