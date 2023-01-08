using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Brands.Exceptions.Application;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Categories.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Features.GettingProductById.v1;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using ECommerce.Services.Catalogs.Suppliers;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;
using FluentValidation;
using MediatR;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct.v1;

public record UpdateProduct(
    long Id,
    string Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null) : ITxUpdateCommand;

internal class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class UpdateProductInvalidateCache : InvalidateCacheRequest<UpdateProduct>
{
    public override IEnumerable<string> CacheKeys(UpdateProduct request)
    {
        yield return $"{Prefix}{nameof(GetProductById)}_{request.Id}";
    }
}

internal class UpdateProductCommandHandler : ICommandHandler<UpdateProduct>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public UpdateProductCommandHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Unit> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var product = await _catalogDbContext.FindProductByIdAsync(ProductId.Of(command.Id));
        Guard.Against.NotFound(product, new ProductNotFoundException(command.Id));

        var category = await _catalogDbContext.FindCategoryAsync(CategoryId.Of(command.CategoryId));
        Guard.Against.NotFound(category, new CategoryNotFoundException(command.CategoryId));

        var brand = await _catalogDbContext.FindBrandAsync(BrandId.Of(command.BrandId));
        Guard.Against.NotFound(brand, new BrandNotFoundException(command.BrandId));

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(SupplierId.Of(command.SupplierId));
        Guard.Against.NotFound(supplier, new SupplierNotFoundException(command.SupplierId));

        product!.ChangeCategory(CategoryId.Of(command.CategoryId));
        product.ChangeBrand(BrandId.Of(command.BrandId));
        product.ChangeSupplier(SupplierId.Of(command.SupplierId));

        product.ChangeDescription(command.Description);
        product.ChangeName(Name.Of(command.Name));
        product.ChangePrice(Price.Of(command.Price));
        product.ChangeSize(Size.Of(command.Size));
        product.ChangeStatus(command.Status);
        product.ChangeDimensions(Dimensions.Of(command.Width, command.Height, command.Depth));
        product.ChangeMaxStockThreshold(command.MaxStockThreshold);
        product.ChangeRestockThreshold(command.RestockThreshold);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
