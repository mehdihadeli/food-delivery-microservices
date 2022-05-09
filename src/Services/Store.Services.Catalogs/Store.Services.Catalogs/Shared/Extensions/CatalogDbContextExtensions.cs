using Store.Services.Catalogs.Brands;
using Store.Services.Catalogs.Categories;
using Store.Services.Catalogs.Products.Models;
using Store.Services.Catalogs.Products.ValueObjects;
using Store.Services.Catalogs.Shared.Contracts;
using Store.Services.Catalogs.Suppliers;

namespace Store.Services.Catalogs.Shared.Extensions;

/// <summary>
/// Put some shared code between multiple feature here, for preventing duplicate some codes
/// Ref: https://www.youtube.com/watch?v=01lygxvbao4.
/// </summary>
public static class CatalogDbContextExtensions
{
    public static Task<bool> ProductExistsAsync(
        this ICatalogDbContext context,
        ProductId id,
        CancellationToken cancellationToken = default)
    {
        return context.Products.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Product?> FindProductByIdAsync(
        this ICatalogDbContext context,
        ProductId id)
    {
        return context.Products.FindAsync(id);
    }

    public static Task<bool> SupplierExistsAsync(
        this ICatalogDbContext context,
        SupplierId id,
        CancellationToken cancellationToken = default)
    {
        return context.Suppliers.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Supplier?> FindSupplierByIdAsync(
        this ICatalogDbContext context,
        SupplierId id)
    {
        return context.Suppliers.FindAsync(id);
    }

    public static Task<bool> CategoryExistsAsync(
        this ICatalogDbContext context,
        CategoryId id,
        CancellationToken cancellationToken = default)
    {
        return context.Categories.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Category?> FindCategoryAsync(
        this ICatalogDbContext context,
        CategoryId id)
    {
        return context.Categories.FindAsync(id);
    }

    public static Task<bool> BrandExistsAsync(
        this ICatalogDbContext context,
        BrandId id,
        CancellationToken cancellationToken = default)
    {
        return context.Brands.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Brand?> FindBrandAsync(
        this ICatalogDbContext context,
        BrandId id)
    {
        return context.Brands.FindAsync(id);
    }
}
