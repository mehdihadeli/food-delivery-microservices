using Store.Services.Identity.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Store.Services.Identity.Shared.Data.EntityConfigurations;

internal class UserOldPasswordConfiguration : IEntityTypeConfiguration<UserOldPassword>
{
    public void Configure(EntityTypeBuilder<UserOldPassword> builder)
    {
        builder.ToTable("UserOldPasswords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SetAt).IsRequired();

        builder.HasOne(uop => uop.User).WithMany().HasForeignKey(uop => uop.UserId);
    }
}
