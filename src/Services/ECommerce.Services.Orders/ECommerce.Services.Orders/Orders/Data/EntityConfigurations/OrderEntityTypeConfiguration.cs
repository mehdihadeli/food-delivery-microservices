using BuildingBlocks.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Services.Orders.Orders.Models;
using ECommerce.Services.Orders.Shared.Data;

namespace ECommerce.Services.Orders.Orders.Data.EntityConfigurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", OrdersDbContext.DefaultSchema);

        builder.HasKey(c => c.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => id)
            .ValueGeneratedNever();

        builder.OwnsOne(m => m.Customer, a =>
        {
            a.Property(p => p.Name)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Property(p => p.CustomerId);
        });

        builder.OwnsOne(m => m.Product, a =>
        {
            a.Property(p => p.Name)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Property(p => p.Price);

            a.Property(p => p.ProductId);
        });
    }
}
