using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Services.Catalogs.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Services.Catalogs.Brands.Data;

public class BrandEntityConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable(nameof(Brand).Pluralize().Underscore(), CatalogDbContext.DefaultSchema);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.OwnsOne(
            x => x.Name,
            a =>
            {
                // configuration just for changing column name in db (instead of name_value)
                a.Property(p => p.Value).HasColumnName(nameof(Brand.Name).Underscore()).IsRequired();
            }
        );
        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
