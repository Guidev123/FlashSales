using FlashSales.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Outbox
{
    public sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
    {
        public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
        {
            builder.ToTable("OutboxMessageConsumers");

            builder.HasKey(c => c.Id);

            builder.HasIndex(c => new { c.OutboxMessageId, c.Name }).IsUnique();

            builder.Property(c => c.Name)
                .HasColumnType("VARCHAR(256)")
                .IsRequired();

            builder.Property(c => c.OutboxMessageId)
                .IsRequired();

            builder.HasOne<OutboxMessage>()
                .WithMany()
                .HasForeignKey(nameof(OutboxMessageConsumer.OutboxMessageId))
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}