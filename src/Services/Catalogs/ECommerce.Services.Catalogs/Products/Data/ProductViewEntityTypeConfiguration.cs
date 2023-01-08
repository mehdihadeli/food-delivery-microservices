using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Services.Catalogs.Products.Data;

public class ProductViewEntityTypeConfiguration : IEntityTypeConfiguration<ProductView>
{
    public void Configure(EntityTypeBuilder<ProductView> builder)
    {
        builder.ToTable(nameof(ProductView).Pluralize().Underscore(), CatalogDbContext.DefaultSchema);
        builder.HasKey(x => x.ProductId);
        builder.HasIndex(x => x.ProductId).IsUnique();
    }
}
