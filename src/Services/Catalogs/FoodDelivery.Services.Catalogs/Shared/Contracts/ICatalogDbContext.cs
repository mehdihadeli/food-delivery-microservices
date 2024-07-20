using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Contracts;

public interface ICatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<ProductView> ProductsView { get; }

    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
