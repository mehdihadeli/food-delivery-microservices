using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.Persistence.EfCore;
using Store.Services.Catalogs.Brands;
using Store.Services.Catalogs.Categories;
using Store.Services.Catalogs.Products.Models;
using Store.Services.Catalogs.Shared.Contracts;
using Store.Services.Catalogs.Suppliers;

namespace Store.Services.Catalogs.Shared.Data;

public class CatalogDbContext : EfDbContextBase, ICatalogDbContext
{
    public const string DefaultSchema = "catalog";

    public CatalogDbContext(DbContextOptions options) : base(options)
    {
    }

    public CatalogDbContext(DbContextOptions options, IDomainEventPublisher domainEventPublisher)
        : base(options, domainEventPublisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductView> ProductsView => Set<ProductView>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Brand> Brands => Set<Brand>();
}
