using FlashSales.Application.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Inbox
{
    public sealed class InboxMessageConsumerConfiguration : IEntityTypeConfiguration<InboxMessageConsumer>
    {
        public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
        {
            builder.ToTable("InboxMessageConsumers");
            builder.HasKey(c => c.Id);

            builder.HasIndex(c => new { c.InboxMessageId, c.Name }).IsUnique();

            builder.Property(c => c.Name)
                .HasColumnType("VARCHAR(256)")
                .IsRequired();

            builder.Property(c => c.InboxMessageId)
                .IsRequired();

            builder.HasOne<InboxMessage>()
                .WithMany()
                .HasForeignKey(nameof(InboxMessageConsumer.InboxMessageId))
                .HasPrincipalKey(nameof(InboxMessage.CorrelationId))
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}