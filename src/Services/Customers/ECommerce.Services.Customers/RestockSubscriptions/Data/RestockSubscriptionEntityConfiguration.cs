using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Services.Customers.RestockSubscriptions.Data;

public class RestockSubscriptionEntityConfiguration : IEntityTypeConfiguration<RestockSubscription>
{
    public void Configure(EntityTypeBuilder<RestockSubscription> builder)
    {
        builder.ToTable(nameof(RestockSubscription).Pluralize().Underscore(), CustomersDbContext.DefaultSchema);

        // ids will use strongly typed-id value converter selector globally
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(x => x.Processed).HasDefaultValue(false);

        builder.Property(c => c.CustomerId);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId);

        builder.OwnsOne(x => x.ProductInformation);

        builder.OwnsOne(
            x => x.Email,
            a =>
            {
                // configuration just for  changing column name in db (instead of email_value)
                a.Property(p => p.Value)
                    .HasColumnName(nameof(RestockSubscription.Email).Underscore());
            });

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
