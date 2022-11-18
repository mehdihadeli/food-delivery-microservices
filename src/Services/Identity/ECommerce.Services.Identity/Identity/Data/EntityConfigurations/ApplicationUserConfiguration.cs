using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Identity.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Services.Identity.Identity.Data.EntityConfigurations;

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(15).IsRequired(false);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql(EfConstants.DateAlgorithm);

        builder.Property(x => x.UserState)
            .HasDefaultValue(UserState.Active)
            .HasConversion(x => x.ToString(), x => (UserState)Enum.Parse(typeof(UserState), x));

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();

        // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model#add-navigation-properties
        // Each User can have many entries in the UserRole join table
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}
