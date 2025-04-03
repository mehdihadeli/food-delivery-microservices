using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class MessagePersistenceEntityTypeConfiguration : IEntityTypeConfiguration<PersistMessage>
{
    public void Configure(EntityTypeBuilder<PersistMessage> builder)
    {
        builder.NotBeNull();

        builder.ToTable("MessagePersistence".Underscore(), MessagePersistenceDbContext.DefaultSchema);
        builder.Property(x => x.Id).IsRequired();
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId).IsRequired();

        builder.Property(x => x.RetryCount).HasColumnType("int").HasDefaultValue(0);

        builder
            .Property(x => x.DeliveryType)
            .HasMaxLength(50)
            .HasConversion(v => v.ToString(), v => Enum.Parse<MessageDeliveryType>(v))
            .IsRequired()
            .IsUnicode(false);

        builder
            .Property(x => x.MessageStatus)
            .HasMaxLength(50)
            .HasConversion(v => v.ToString(), v => Enum.Parse<MessageStatus>(v))
            .IsRequired()
            .IsUnicode(false);
    }
}
