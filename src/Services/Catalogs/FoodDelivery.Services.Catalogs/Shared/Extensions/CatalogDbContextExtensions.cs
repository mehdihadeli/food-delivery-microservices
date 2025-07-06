using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions;

/// <summary>
/// Put some shared code between multiple feature here, for preventing duplicate some codes
/// Ref: https://www.youtube.com/watch?v=01lygxvbao4.
/// </summary>
public static class CatalogDbContextExtensions
{
    public static Task<bool> ProductExistsAsync(
        this ICatalogDbContext context,
        ProductId id,
        CancellationToken cancellationToken = default
    )
    {
        return context.Products.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Product?> FindProductByIdAsync(this ICatalogDbContext context, ProductId id)
    {
        return context.Products.FindAsync(id);
    }

    public static Task<bool> SupplierExistsAsync(
        this ICatalogDbContext context,
        SupplierId id,
        CancellationToken cancellationToken = default
    )
    {
        return context.Suppliers.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Supplier?> FindSupplierByIdAsync(this ICatalogDbContext context, SupplierId id)
    {
        return context.Suppliers.FindAsync(id);
    }

    public static Supplier? FindSupplierById(this ICatalogDbContext context, SupplierId id)
    {
        return context.Suppliers.Find(id);
    }

    public static Task<bool> CategoryExistsAsync(
        this ICatalogDbContext context,
        CategoryId id,
        CancellationToken cancellationToken = default
    )
    {
        return context.Categories.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Category?> FindCategoryAsync(this ICatalogDbContext context, CategoryId id)
    {
        return context.Categories.FindAsync(id);
    }

    public static Category? FindCategory(this ICatalogDbContext context, CategoryId id)
    {
        return context.Categories.Find(id);
    }

    public static Task<bool> BrandExistsAsync(
        this ICatalogDbContext context,
        BrandId id,
        CancellationToken cancellationToken = default
    )
    {
        return context.Brands.AnyAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public static ValueTask<Brand?> FindBrandAsync(this ICatalogDbContext context, BrandId id)
    {
        return context.Brands.FindAsync(id);
    }

    public static Brand? FindBrand(this ICatalogDbContext context, BrandId id)
    {
        return context.Brands.Find(id);
    }
}
