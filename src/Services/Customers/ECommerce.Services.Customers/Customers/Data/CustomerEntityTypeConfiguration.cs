using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.Shared.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Services.Customers.Customers.Data;

public class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(nameof(Customer).Pluralize().Underscore(), CustomersDbContext.DefaultSchema);

        // ids will use strongly typed-id value converter selector globally
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(x => x.IdentityId);
        builder.HasIndex(x => x.IdentityId).IsUnique();

        builder.OwnsOne(
            x => x.Email,
            a =>
            {
                a.Property(p => p.Value)
                    .HasColumnName(nameof(Customer.Email).Underscore())
                    .IsRequired()
                    .HasMaxLength(EfConstants.Lenght.Medium);

                // supporting index on owned types:
                // https://github.com/dotnet/efcore/issues/11336
                // https://github.com/dotnet/efcore/issues/12637
                a.HasIndex(p => p.Value).IsUnique();
            });

        builder.OwnsOne(
            x => x.PhoneNumber,
            a =>
            {
                a.Property(p => p.Value)
                    .HasColumnName(nameof(Customer.PhoneNumber).Underscore())
                    .IsRequired()
                    .HasMaxLength(EfConstants.Lenght.Tiny);

                // supporting index on owned types:
                // https://github.com/dotnet/efcore/issues/11336
                // https://github.com/dotnet/efcore/issues/12637
                a.HasIndex(p => p.Value).IsUnique();
            });

        builder.OwnsOne(m => m.Name);

        builder.OwnsOne(
            m => m.Address,
            a =>
            {
                a.Property(p => p.City)
                    .HasMaxLength(EfConstants.Lenght.Short);
                a.Property(p => p.Country)
                    .HasMaxLength(EfConstants.Lenght.Medium);
                a.Property(p => p.Detail)
                    .HasMaxLength(EfConstants.Lenght.Medium);
                a.Property(p => p.PostalCode)
                    .HasConversion(s => s.Value, v => new PostalCode {Value = v})
                    .IsRequired(false);
            });

        builder.OwnsOne(
            x => x.Nationality,
            a =>
            {
                // configuration just for  changing column name in db (instead of nationality_value)
                a.Property(x => x.Value)
                    .HasColumnName(nameof(Customer.Nationality).Underscore())
                    .HasMaxLength(EfConstants.Lenght.Short);
            });

        builder.OwnsOne(
            x => x.BirthDate,
            a =>
            {
                // configuration just for  changing column name in db (instead of birthDate_value)
                a.Property(x => x.Value)
                    .HasColumnName(nameof(Customer.BirthDate).Underscore());
            });

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
