using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : EfDbContextBase(options), ICatalogDbContext
{
    public const string DefaultSchema = "catalog";

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProductView> ProductsView { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Supplier> Suppliers { get; set; } = default!;
    public DbSet<Brand> Brands { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
