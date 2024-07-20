using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Services.Catalogs.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Services.Catalogs.Categories.Data;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(nameof(Category).Pluralize().Underscore(), CatalogDbContext.DefaultSchema);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.OwnsOne(
            ci => ci.Name,
            a =>
            {
                // configuration just for changing column name in db (instead of name_value)
                a.Property(p => p.Value).HasColumnName(nameof(Category.Name).Underscore()).IsRequired();
            }
        );

        builder.OwnsOne(
            ci => ci.Code,
            a =>
            {
                // configuration just for changing column name in db (instead of code_value)
                a.Property(p => p.Value).HasColumnName(nameof(Category.Code).Underscore()).IsRequired();
            }
        );

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
