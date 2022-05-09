using BuildingBlocks.Core.Persistence.EfCore;
using Store.Services.Catalogs.Brands;
using Store.Services.Catalogs.Categories;
using Store.Services.Catalogs.Products.Models;
using Store.Services.Catalogs.Products.ValueObjects;
using Store.Services.Catalogs.Shared.Data;
using Store.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Store.Services.Catalogs.Products.Data;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", CatalogDbContext.DefaultSchema);

        builder.HasKey(c => c.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnType(EfConstants.ColumnTypes.NormalText)
            .HasConversion(name => name.Value, name => Name.Create(name))
            .IsRequired();

        builder.Property(ci => ci.Price)
            .HasColumnType(EfConstants.ColumnTypes.PriceDecimal)
            .HasConversion(price => price.Value, price => price)
            .IsRequired();

        builder.Property(ci => ci.Size)
            .HasConversion(size => size.Value, size => Size.Create(size))
            .IsRequired();

        builder.Property(x => x.ProductStatus)
            .HasDefaultValue(ProductStatus.Available)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(
                x => x.ToString(),
                x => (ProductStatus)Enum.Parse(typeof(ProductStatus), x));

        builder.Property(x => x.Color)
            .HasDefaultValue(ProductColor.Black)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(
                x => x.ToString(),
                x => (ProductColor)Enum.Parse(typeof(ProductColor), x));

        builder.OwnsOne(c => c.Dimensions, cm =>
        {
            cm.Property(c => c.Height);
            cm.Property(c => c.Width);
            cm.Property(c => c.Depth);
        });

        builder.OwnsOne(c => c.Stock, cm =>
        {
            cm.Property(c => c.Available);
            cm.Property(c => c.RestockThreshold);
            cm.Property(c => c.MaxStockThreshold);
        });

        builder.Property(x => x.CategoryId)
            .HasConversion(categoryId => categoryId.Value, categoryId => categoryId);

        builder.HasOne<Category>(x => x.Category)
            .WithMany()
            .HasForeignKey(x => (long)x.CategoryId);

        builder.Property(x => x.BrandId)
            .HasConversion(brandId => brandId.Value, brandId => brandId);

        builder.HasOne<Brand>(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId);

        builder.Property(x => x.SupplierId)
            .HasConversion(supplierId => supplierId.Value, supplierId => supplierId);

        builder.HasOne<Supplier>(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId);

        builder.HasMany(s => s.Images)
            .WithOne(s => s.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
