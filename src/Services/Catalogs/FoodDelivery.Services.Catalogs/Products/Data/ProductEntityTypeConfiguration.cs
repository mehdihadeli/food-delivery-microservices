using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Services.Catalogs.Products.Data;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(nameof(Product).Pluralize().Underscore(), CatalogDbContext.DefaultSchema);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        // OwnsOne calling ValueObject private constructor, for skipping value object validation during ef core materialization process, this is not possible with using `builder.Property` because we can't call provate constructor directly and we should use `Of`.
        builder.OwnsOne(
            ci => ci.Name,
            a =>
            {
                // configuration just for changing column name in db (instead of name_value)
                a.Property(p => p.Value).HasColumnName(nameof(Product.Name).Underscore()).IsRequired();
            }
        );

        builder.OwnsOne(
            ci => ci.ProductInformation,
            a =>
            {
                a.Property(p => p.Content).IsRequired();
            }
        );

        builder.OwnsOne(
            ci => ci.Price,
            a =>
            {
                // configuration just for changing column name in db (instead of price_value)
                a.Property(p => p.Value)
                    .HasColumnType(EfConstants.ColumnTypes.PriceDecimal)
                    .HasColumnName(nameof(Product.Price).Underscore())
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            ci => ci.Size,
            a =>
            {
                // configuration just for  changing column name in db (instead of size_value)
                a.Property(p => p.Value).HasColumnName(nameof(Product.Size).Underscore()).IsRequired();
            }
        );

        builder
            .Property(x => x.ProductStatus)
            .HasDefaultValue(ProductStatus.Available)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(x => x.ToString(), x => (ProductStatus)Enum.Parse(typeof(ProductStatus), x));

        builder
            .Property(x => x.ProductType)
            .HasDefaultValue(ProductType.Food)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(x => x.ToString(), x => (ProductType)Enum.Parse(typeof(ProductType), x));

        builder
            .Property(x => x.Color)
            .HasDefaultValue(ProductColor.Black)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(x => x.ToString(), x => (ProductColor)Enum.Parse(typeof(ProductColor), x));

        builder.OwnsOne(c => c.Dimensions);

        builder.OwnsOne(c => c.Stock);

        builder.Property(x => x.CategoryId);

        builder.HasOne(x => x.Category).WithMany().HasForeignKey(product => product.CategoryId);

        builder.Property(x => x.BrandId);

        builder.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId);

        builder.Property(x => x.SupplierId);

        builder.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);

        builder
            .HasMany(s => s.Images)
            .WithOne(s => s.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
